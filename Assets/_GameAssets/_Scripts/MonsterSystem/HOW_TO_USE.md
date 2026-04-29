# MonsterSystem — How To Use

The whole system is just a way to write monster behaviour in small, reusable pieces instead of one giant script.
Think of every class here like a MonoBehaviour, just split up so each piece only does one thing.

---

## The Big Picture

A monster is made of three parts:

```
[Monster Root]
  ├── MonsterController       ← the hub. holds everything together
  ├── [States]                ← child GameObject, one state per child
  │     ├── IdleState
  │     ├── ChaseState
  │     └── KillState
  └── [Sensors]               ← child GameObject, one sensor per child
        ├── PlayerProximitySensor
        └── RadioSensor
```

- **States** are what the monster is *doing* right now. Only one is active at a time.
- **Sensors** watch the world and say "now switch to this state".
- **MonsterController** is the hub that connects them. You rarely need to add to it.

---

## MonsterState

> Think of it exactly like a MonoBehaviour.

| MonoBehaviour | MonsterState |
|---|---|
| `Awake` / `Start` | `Initialize(MonsterController)` |
| `OnEnable` | `OnStateEnter()` |
| `OnDisable` | `OnStateExit()` |

Each state is its own **GameObject** under the `States` child. The controller enables and disables
these GameObjects as it switches between states, so the state only "exists" while it is active.

**The basics:**

```csharp
public class MyState : MonsterState
{
    // Called once at startup — same idea as Awake/Start.
    // Cache your references here. Always call base first.
    public override void Initialize(MonsterController owningController)
    {
        base.Initialize(owningController);
        // controller is now set — use it to get sensors, animator, etc.
    }

    // Monster just entered this state — same idea as OnEnable.
    public override void OnStateEnter()
    {
        // start navmesh, play sound, set a flag, whatever this state needs to do
    }

    // Monster is leaving this state — same idea as OnDisable.
    public override void OnStateExit()
    {
        // clean up: stop navmesh, cancel coroutines, etc.
    }
}
```

**Things available inside any state via `this.controller`:**

```csharp
controller.PlayerTarget      // Transform of the player
controller.Animator          // the monster's Animator
controller.Audio             // the monster's AudioSource
controller.CurrentNight      // which night it is (for scaling difficulty)
controller.GetSensor<T>()    // find a sensor by type
controller.CurrentState      // what state is active right now
controller.PreviousState     // what state was active before this one
```

**To switch to another state from inside a state:**

```csharp
[SerializeField] private MonsterState nextState;

// anywhere in OnStateEnter / OnStateExit / a callback:
RequestTransition(nextState);
```

**`BlocksTransitions`** — tick this in the Inspector to stop sensors from interrupting this state.
Use it on states like a kill state that must never be cut short.

---

## MonsterSensor

> A sensor watches for a condition and tells the monster when to switch states.

Sensors are ticked every ~0.2 s by the system (not every frame). Use them for polling
(distance checks, timer checks, etc.). For instant reactions use events in `Subscribe`.

```csharp
public class MyProximitySensor : MonsterSensor
{
    [SerializeField] private MonsterState chaseState;
    [SerializeField] private float range = 5f;

    // Subscribe is like OnEnable — wire up events here.
    protected override void Subscribe()
    {
        // e.g. SomeSystem.OnSomething += HandleSomething;
    }

    // Unsubscribe is like OnDisable — unwire events here.
    protected override void Unsubscribe()
    {
        // e.g. SomeSystem.OnSomething -= HandleSomething;
    }

    // OnTick runs every ~0.2 s — use it for polling.
    // Always call base first so TickDelta is set.
    public override void OnTick(float tickDelta)
    {
        base.OnTick(tickDelta);

        float dist = Vector3.Distance(transform.position, controller.PlayerTarget.position);
        if (dist < range)
            TriggerTransitionTo(chaseState); // fires once, resets after next state change
    }
}
```

**Transition helpers available in every sensor:**

```csharp
TriggerStateTransition();           // goes to the state set in the Inspector field on the sensor
TriggerTransitionTo(someState);     // goes to a specific state
TriggerTransitionTo(someState, context); // same but passes data to IStateWithContext<T>
```

The sensor automatically resets after each state change, so it can fire again next time.

---

## MonsterController

You almost never need to modify this. It:

- Collects all states and sensors automatically from child GameObjects at startup.
- Calls `Initialize` on all of them in `Start`.
- Drives `TransitionTo` when a state or sensor requests a change.
- Registers with `MonsterStateManager` so sensors get ticked.

The only time you touch it is to add a new serialized reference (like a new config SO) that
multiple states need.

---

## Affordances (audio, animation, VFX)

Affordances are **components you add to the same GameObject as a state**. They handle
the "presentation" side — playing sounds, triggering animator bools, spawning VFX — so
the state logic itself stays clean.

| Affordance | What it does |
|---|---|
| `AudioAffordance` | plays / stops an AudioClip |
| `AnimationAffordance` | sets a parameter or trigger on the Animator |
| `VfxAffordance` | plays a particle system |

**TriggerMode** on the affordance controls when it fires:

- `OnStateEnter` — fires automatically when the state activates. No code needed.
- `OnStateExit` — fires automatically when the state deactivates.
- `Custom` — you call it yourself from inside the state.

Calling it yourself:

```csharp
TriggerAffordances<AudioAffordance>();   // fire all Custom audio affordances on this state
StopAffordances<AudioAffordance>();      // stop all audio affordances, regardless of mode
```

`StopOnExit` on the affordance (ticked by default) means it auto-stops when the state exits,
so you usually don't need to call `StopAffordances` manually.

---

## AnimatedState

> **Important:** AnimatedState is NOT for playing an animation.
> It is for **reacting** to something that happens *inside* an animation.

The animation itself is triggered by an `AnimationAffordance` on the same GameObject
(just like any other state). What `AnimatedState` adds is the ability to get a callback
at a specific point in that animation — for example, "do something at the bite frame",
or "transition out when the animation finishes".

This works through a Unity StateMachineBehaviour called `AnimationStateChange` that you
add to the Animator state. It fires `InvokeAnimationEvent(index)` on the active state
at a configured normalized time.

**The default — transition out when the animation ends:**

```csharp
public class MyAnimatedState : AnimatedState
{
    // Index 0 is already wired to OnAnimationComplete, which transitions to nextState.
    // Just set the nextState field in the Inspector. Nothing else needed.
}
```

**Adding a mid-animation callback (e.g. spawn damage at impact frame):**

```csharp
public class MyAttackState : AnimatedState
{
    protected override void RegisterAnimationEvents()
    {
        base.RegisterAnimationEvents();   // index 0 = OnAnimationComplete (keep this)
        RegisterAnimationEvent(OnImpact); // index 1 = fires at the impact frame
    }

    private void OnImpact()
    {
        // deal damage, play a hit sound, etc.
    }
}
```

Then in the Animator, add a second `AnimationStateChange` SMB to the same state and set
its index to `1`. The first SMB (index 0) handles the end-of-animation transition as normal.

`exitOnComplete` in the Inspector: if unchecked, `OnAnimationComplete` won't transition
automatically — useful when another system decides when to leave (e.g. a looping idle).

---

## MonsterStateWithTimer

A `MonsterState` that adds a built-in repeating timer with an optional stop condition.

```csharp
public class MyTimedState : MonsterStateWithTimer
{
    // Fires every `interval` seconds (set in Inspector under "Timer Configuration")
    protected override void OnTimerTick()
    {
        float elapsed = GetTime(); // seconds since state entered
        // do something every tick
    }

    // Fires once when `duration` seconds have passed (0 = never stops)
    protected override void OnTimerFinished()
    {
        RequestTransition(someNextState);
    }
}
```

The timer starts automatically in `OnStateEnter` and pauses in `OnStateExit`.
Always call `base.OnStateEnter()` and `base.OnStateExit()` or the timer won't work.

---

## IStateWithContext\<T\>

Sometimes a state needs to know *what* triggered it — e.g. which food Rigidbody the
player just handed to the monster. Implement this interface to receive that data.

```csharp
public class MyState : MonsterState, IStateWithContext<Rigidbody>
{
    private Rigidbody receivedData;

    // Called by the system right before OnStateEnter.
    public void ReceiveContext(Rigidbody context)
    {
        this.receivedData = context;
    }

    public override void OnStateEnter()
    {
        // receivedData is ready here
    }
}
```

The sender passes the context when requesting the transition:

```csharp
RequestTransition(myState, someRigidbody);          // from inside a state
TriggerTransitionTo(myState, someRigidbody);        // from inside a sensor
```

---

## Quick Reference

```
Want to...                              Use...
──────────────────────────────────────────────────────────────────
React to entering / leaving a state    OnStateEnter / OnStateExit
Cache references at startup            Initialize (call base first)
Watch for a condition every ~0.2s      MonsterSensor → OnTick
React to an event instantly            MonsterSensor → Subscribe/Unsubscribe
Switch states from inside a state      RequestTransition(targetState)
Switch states from a sensor            TriggerTransitionTo(targetState)
Pass data between states               IStateWithContext<T>
Play audio / animation on enter        StateAffordance (OnStateEnter mode)
Play audio / animation mid-state       TriggerAffordances<AudioAffordance>()
React to a frame inside an animation   AnimatedState + AnimationStateChange SMB
Run logic on an interval / timeout     MonsterStateWithTimer
Prevent sensors from interrupting      BlocksTransitions = true (Inspector)
```
