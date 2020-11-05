term.setSize(51, 19, 3)
while true do
	term.write("> ")
	local input = term.read()
	
	if input == "hello" then
		print("Hello, World!")
	end
end