local timer = require("timer")
while true do
	local time = math.random(500, 5000)

	local id = timer.start(-1)
	print("Started", id, time)
	repeat
		local ev, timerId = coroutine.yield()
	until ev == "timer" and timerId == id
	print("Elapsed", id)
end