local event = require("event")
local expect = require("expect")

local function run(threads, waitAll)
	local count = #threads
	local filters = {}
	local ev = {n = 0}
	while true do
		for i = 1, count do
			local t = threads[i]
			if t then
				if filters[i] == nil or filters[i] == ev[1] or filters[i] == "interrupt" then
					local ok, par = coroutine.resume(t, table.unpack(ev, 1, ev.n))
					if ok then
						filters[i] = par
					else
						error(par, 0)
					end

					if coroutine.status(t) == "dead" then
						threads[i] = nil
						
						if not waitAll then
							return
						end
					end
				end
			end
		end
		ev = table.pack(coroutine.yield())
	end
end

local function waitForAny(...)
	local threads = {}
	for i, v in ipairs({...}) do
		expect(i, v, "function")
		table.insert(threads, coroutine.create(v))
	end
	return run(threads, false)
end

local function waitForAll(...)
	local threads = {}
	for i, v in ipairs({...}) do
		expect(i, v, "function")
		table.insert(threads, coroutine.create(v))
	end
	return run(threads, true)
end

return {
	waitForAny = waitForAny,
	waitForAll = waitForAll
}
