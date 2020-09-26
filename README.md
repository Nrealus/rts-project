**An old and unsupported repository of a Unity 3D project that I don't remember that much about**

The main point of interest of the project was wheeled vehicle pathfinding and locomotion (pathtracking) in 3D using forces and colliders.
So although the subject is more or less related to autonomous vehicles, the "demo application" of is a sort of RTS unit movement command scenario.

A solo agent does perform in a surprisingly nice manner, as the very simple (!) rules it was given for path tracking did allow it to follow the path, and maneuver if needed ! (niche-like maneuver if overshot next position...)
But this comes at the cost of having to tailor a few (but easy to understand) numeric parameters, like the minimal/maximal turn radius, wheel turn angle etc.

**But if there was one thing to take away from this project**, it would probably the (easy and simple!) system used to decide where and how much to steer to.
In short, there are two circles each of them tangent to a side of the car. When the next position to follow is in one of the circle, the car slows down and it's steering is inverted.
When the point is out of those two circles, it means that the car can easily reach it, as it's not within its minimal turn radius.

The use of this very simple idea had great results, but there was some parameter tailoring to do with the radiuses.

Also, this idea could be used elsewhere in the system to sophisticate the whole thing. For instance, in front or on the back of the car. Indeed, there is also support for reverse movement.
Possibly, we could imagine a situation where some circles overlap, and where a subsystem decides what to choose. (in a probabilistic manner ? or with machine learning ? or just with simple algebra (like steering behaviours ?))

Another goal I had but didn't reach completely successfully was vehicle formations support. Indeed, the pathfinding library used didn't really allow me
to calculate correct reachable positions from the unreachable positions offset from the formation leader. Because of that, I couldn't really 

But if that could have been done, further addition of a few rules based on steering behaviours and local avoidance would have been promising !

