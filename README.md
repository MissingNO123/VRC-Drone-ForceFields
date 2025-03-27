# VRChat Drone Force Fields
A simple script I made to create force fields that can interact with players' Drones.

### Download >>[HERE](https://github.com/MissingNO123/VRC-Drone-ForceFields/releases/download/1.0.0/DroneForceFields_1.0.unitypackage)<<

The force field can be configured to push drones in a direction, push them away from a point, or teleport the drone on entry.
I have designed the system to be both powerful and easy to use. 

The most obvious use case is to create a zone that drones cannot enter.
It can ignore players who are inside the zone, so that for example players within a private room can still use them from inside, but players outside cannot fly their drones inside.

It can be configured to push the drone away, suck it in to a specific point, or boost it in a direction. 
For example, you could place boost rings around a race track, and place walls that guide the drone back on track like bowling gutters.

Other scripts can be registered as Event Listeners - as in, when the drone enters or exits the zone, it will send the events `OnDroneForcefieldEnter` and `OnDroneForcefieldExit` respectively to all registered UdonBehaviours. This can be used to trigger animations, for example.

I tried to cram as many options as I can for you to do whatever you want with it. Go get creative.

## Demonstration
An example world that demonstrates everything can be found [here](https://vrchat.com/home/world/wrld_1eedec62-27ec-44e8-b3d7-176fede1b1e1).
