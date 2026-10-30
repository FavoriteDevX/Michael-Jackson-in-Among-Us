# Sounds

This folder is intentionally empty in the repository. **No audio is shipped with this mod.**

Copy this folder to `<Among Us>/BepInEx/plugins/MichaelJacksonRole/Sounds/` and drop your own
`.wav` files into it:

| File                 | When it plays                                   | Required |
|----------------------|-------------------------------------------------|----------|
| `hee_hee.wav`        | When the Michael Jackson player kills someone   | No       |
| `ow.wav`             | Right after `hee_hee.wav` on a kill             | No       |
| `thriller_laugh.wav` | When the Michael Jackson player enters/exits a vent | No   |

Missing files are skipped silently (a warning is written to the BepInEx log).

Supported formats: uncompressed WAV, 8/16/24/32-bit integer PCM or 32-bit float, mono or stereo.
Keep clips short (under ~3 seconds) so they don't overlap gameplay audio.
