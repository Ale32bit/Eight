print("Eight 2 Alpha")

-- very bare bone lua repl
while true do
	local input = coroutine.yield()
	input = input:gsub("^=", "return ")
	local func, err = load(input)
	if not func then
		local func2, err2 = load("return " .. input)
		if not func2 then
			print(err)
		else
			local res = table.pack(pcall(func2))
			print(table.unpack(res, 2))
		end
	else 
		local res = table.pack(pcall(func))
		print(table.unpack(res, 2))
	end
end