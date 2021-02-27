local audio = require("audio")
local timer = require("timer")

function audio.wait(freq, dur, vol)
	audio.beep(freq, dur, vol)
	timer.sleep(dur)
end