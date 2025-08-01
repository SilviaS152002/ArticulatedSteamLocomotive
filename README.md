Lets you re-parent the couplers. Should be enough to jerry-rig an articulated into the game.

How to use:

Change `"kind": "SteamLocomotive"` to `"kind": "ArticulatedSteamLocomotive"`

then add this in `"definition"`:
```
        "couplers": {
          "couplerParentF": {
            "path": [ /*same as path to wheelset*/ ]
          },
          "couplerParentR": null, // Can also be null, meaning it won't be touched
          "offsetF": 2.06679, // PositionHead - Z-value of local position of the wheelset w.r.t. model
          "offsetR": 0
        }
```
