using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using UDL.Services.Aredl;

void Check(bool condition,string message) { if(!condition) throw new Exception(message); }
async Task Until(Func<bool> condition)
{
    using var timeout=new CancellationTokenSource(TimeSpan.FromSeconds(5));
    while(!condition()) await Task.Delay(5,timeout.Token);
}
foreach(var mode in new[]{"--sync-aredl","--import-udl-sheet","--verify-database","--grant-role"})
    Check(!AredlSyncBackgroundService.ShouldRegister([mode],"Development"),"Comando manual excluido");
Check(!AredlSyncBackgroundService.ShouldRegister([],"Testing"),"Testing excluido");
Check(AredlSyncBackgroundService.ShouldRegister([],"Development"),"Web Development permitida");
var state=new State();
var services=new ServiceCollection(); services.AddScoped<IAredlSyncService>(_=>new FakeSync(state));
using var provider=services.BuildServiceProvider();
var clock=new Clock();
AredlSyncBackgroundService Worker(AredlOptions options)=>new(provider.GetRequiredService<IServiceScopeFactory>(),Options.Create(options),clock,NullLogger<AredlSyncBackgroundService>.Instance);
using(var disabled=Worker(new(){SyncEnabled=false}))
{
    await disabled.StartAsync(default); await disabled.ExecuteTask!;
    Check(state.Calls==0 && clock.Pending==0,"SyncEnabled=false sin reloj ni sync");
}
state.FailFirst=true;
using(var worker=Worker(new(){SyncInitialDelaySeconds=5,SyncIntervalMinutes=30}))
{
    await worker.StartAsync(default); await Until(()=>clock.Pending==1);
    Check(state.Calls==0,"No bloquea arranque esperando API");
    clock.Advance(TimeSpan.FromSeconds(4)); Check(state.Calls==0,"Espera inicial configurada");
    clock.Advance(TimeSpan.FromSeconds(1)); await Until(()=>state.Disposed==1 && clock.Pending==1);
    Check(state.Calls==1 && !worker.ExecuteTask!.IsCompleted,"Error no termina worker/host");
    clock.Advance(TimeSpan.FromMinutes(29)); Check(state.Calls==1,"No adelanta intervalo");
    clock.Advance(TimeSpan.FromMinutes(1)); await Until(()=>state.Disposed==2 && clock.Pending==1);
    Check(state.Calls==2 && state.Success==1,"Reintenta y reutiliza servicio scoped tras fallo");
    state.Gate=new(TaskCreationOptions.RunContinuationsAsynchronously);
    clock.Advance(TimeSpan.FromMinutes(30)); await Until(()=>state.Calls==3);
    clock.Advance(TimeSpan.FromHours(4)); Check(state.Calls==3 && state.MaxActive==1,"Sin ciclos simultáneos ni ticks pendientes");
    await worker.StopAsync(default);
    Check(worker.ExecuteTask!.IsCompletedSuccessfully && state.Disposed==3,"Cancelación propagada, scope liberado");
}
Check(new AredlOptions().SyncIntervalMinutes==60,"Intervalo predeterminado final 60");
Console.WriteLine("OK: exclusión de consola/Testing, SyncEnabled, inicio diferido, intervalo configurable con reloj virtual, reintento tras excepción, sin solapamiento, scopes y apagado cancelable. Sin SQL ni API real.");

sealed class State { public int Calls,Disposed,Success,Active,MaxActive; public bool FailFirst; public TaskCompletionSource? Gate; }
sealed class FakeSync(State state):IAredlSyncService,IDisposable
{
    public async Task<AredlSyncResult> SynchronizeAsync(CancellationToken ct=default)
    {
        state.Calls++; state.Active++; state.MaxActive=Math.Max(state.MaxActive,state.Active);
        try
        {
            if(state.FailFirst && state.Calls==1) throw new HttpRequestException("AREDL caída simulada");
            if(state.Gate is not null) await state.Gate.Task.WaitAsync(ct);
            state.Success++; return new(0,0,10,0,0,10,0,[]);
        }
        finally { state.Active--; }
    }
    public void Dispose()=>state.Disposed++;
}
sealed class Clock:TimeProvider
{
    private readonly List<Timer> timers=[];
    private TimeSpan elapsed;
    public int Pending { get { lock(timers) return timers.Count(t=>!t.Removed); } }
    public override ITimer CreateTimer(TimerCallback callback,object? state,TimeSpan dueTime,TimeSpan period)
    { lock(timers) { var timer=new Timer(this,callback,state,elapsed+dueTime); timers.Add(timer); return timer; } }
    public void Advance(TimeSpan amount)
    {
        List<Timer> ready;
        lock(timers) { elapsed+=amount; ready=timers.Where(t=>!t.Removed && t.Due<=elapsed).ToList(); foreach(var t in ready)t.Removed=true; }
        foreach(var t in ready)t.Callback(t.State);
    }
    private sealed class Timer(Clock clock,TimerCallback callback,object? state,TimeSpan due):ITimer
    {
        public TimerCallback Callback=callback; public object? State=state; public TimeSpan Due=due; public bool Removed;
        public bool Change(TimeSpan dueTime,TimeSpan period) { lock(clock.timers){ Due=clock.elapsed+dueTime; Removed=false; return true; } }
        public void Dispose(){lock(clock.timers)Removed=true;}
        public ValueTask DisposeAsync(){Dispose();return ValueTask.CompletedTask;}
    }
}
