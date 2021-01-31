--[[
	
	TIC-TAC-TOE
	by Evan Hahn (http://www.evanhahn.com/)
	ported to Eight and improved upon by Dimaguy#7491 (Discord)
	
	This is a program that allows you to play tic-tac-toe against the
	computer.(or against other player, or even only computer vs computer)
	
	You may change the configuration (below) to play with "white" and "black" 
	instead of "x" and "o", and change how the board is displayed.
	
	How it works:
	
	The board is represented by a 2D table of spaces. They are filled with
	nil to start. When you play, you put an "x" or an "o" into the table. The
	board is used to keep track of piece locations and to display them. It
	does not calculate wins.
		
	The board also has regions. A region is a place where a player may win
	(horizontal, vertical, or diagonal). It holds pointers to the board table.
	Player 1 is represented by +, and Player 2 by -. Each piece in the region
	increments or decrements the checking of the region. Basically, two X's
	returns as 2. Two O's returns as -2. 3 or -3 is a winning region.
--]]

----------------------------------------------
-- Configuration (change this if you wish!) --
----------------------------------------------


-- Display stuff
local PLAYER_1 = "x"	-- Player 1 is represented by this. Player 1 goes first.
local PLAYER_2 = "o"	-- Player 2 is represented by this.
local EMPTY_SPACE = " "	-- An empty space is displayed like this.
local DISPLAY_HORIZONTAL_SEPARATOR = "-"	-- Horizontal lines look like this.
local DISPLAY_VERTICAL_SEPARATOR = " | "	-- Vertical lines look like this


--[[ 
#######################
####   Game Code   ####
#######################
--]]
--------------
-- Requires --
--------------
local term = require("term")
-------------------------------------------------------
-- More configuration (Default variables and checks) --
-------------------------------------------------------
-- Are they playable by human or computer-controlled?
local PLAYER_1_HUMAN = false
local PLAYER_2_HUMAN = false

-- Board size
local BOARD_RANK = 3	-- The board will be this in both dimensions.

local MAX_BOARD_RANK = 16	-- Won't run above this number. Prevents crashes.

---------------------
-- Board functions --
---------------------

-- get the piece at a given spot
function getPiece(x, y)
	return space[x][y]
end

-- get the piece at a given spot; if nil, return " "
-- this is useful for output.
function getPieceNoNil(x, y)
	if getPiece(x, y) ~= nil then
		return getPiece(x, y)
	else
		return EMPTY_SPACE
	end	
end

-- is that space empty?
function isEmpty(x, y)
	if getPiece(x, y) == nil then
		return true
	else
		return false
	end
end

-- place a piece there, but make sure nothing is there already.
-- if you can't play there, return false.
function placePiece(x, y, piece)
	if isEmpty(x, y) == true then
		space[x][y] = piece
		return true
	else
		return false
	end
end

-- is the game over?
function isGameOver()
	if checkWin() == false then	-- if there is no win...
		for i = 0, (BOARD_RANK - 1) do	-- is the board empty?
			for j = 0, (BOARD_RANK - 1) do
				if isEmpty(i, j) == true then return false end
			end
		end
		return true
	else	-- there is a win; the game is over
		return true
	end
end

-- display the board.
-- this uses the configuration file pretty much entirely.
function displayBoard()
	
	-- find the widest player
	local widest_piece = math.max(string.len(PLAYER_1), string.len(PLAYER_2), string.len(EMPTY_SPACE))
	
	-- display board, top to bottom
	print() -- make sure it starts on a new line
	print("Y")
	for i = (BOARD_RANK - 1), -1, -1 do
		local row = ""	-- start with an empty row
		if i == -1 then	
			print()
			for j = 0, (BOARD_RANK - 1) do	-- generate that row
				local piece = j
				row = row .. piece
				row = row .. string.rep(" ", widest_piece - string.len(piece))
				if j ~= (BOARD_RANK - 1) then
					row = row .. "   "
				end
			end
			term.write("X "..row)
			break
		end
		
		for j = 0, (BOARD_RANK - 1) do	-- generate that row
			local piece = getPieceNoNil(j, i)
			row = row .. piece
			row = row .. string.rep(" ", widest_piece - string.len(piece))
			if j ~= (BOARD_RANK - 1) then
				row = row .. DISPLAY_VERTICAL_SEPARATOR
			end
		end
		term.write(i .. " "..row)	-- output row
		if i ~= 0 then	-- output horizontal line as long as the row
			print()
			local repeats = math.ceil(string.len(row) + string.len(i) / string.len(DISPLAY_HORIZONTAL_SEPARATOR))
			print(string.rep(" ", string.len(BOARD_RANK)) .. string.rep(DISPLAY_HORIZONTAL_SEPARATOR, repeats))
		end
	end
	-- finish off with a line break
	print()
end

--------------------
-- Create regions --
--------------------

-- declare region and a number to increment
region = {}
region_number = 0

-- vertical
for i = 0, (BOARD_RANK - 1) do
	region[region_number] = {}
	for j = 0, (BOARD_RANK - 1) do
		region[region_number][j] = {}
		region[region_number][j]["x"] = i
		region[region_number][j]["y"] = j
	end
	region_number = region_number + 1
end

-- horizontal
for i = 0, (BOARD_RANK - 1) do
	region[region_number] = {}
	for j = 0, (BOARD_RANK - 1) do
		region[region_number][j] = {}
		region[region_number][j]["x"] = j
		region[region_number][j]["y"] = i
	end
	region_number = region_number + 1
end

-- diagonal, bottom-left to top-right
region[region_number] = {}
for i = 0, (BOARD_RANK - 1) do
	region[region_number][i] = {}
	region[region_number][i]["x"] = i
	region[region_number][i]["y"] = i
end
region_number = region_number + 1

-- diagonal, top-left to bottom-right
region[region_number] = {}
for i = (BOARD_RANK - 1), 0, -1 do
	region[region_number][i] = {}
	region[region_number][i]["x"] = BOARD_RANK - i - 1
	region[region_number][i]["y"] = i
end
region_number = region_number + 1

----------------------
-- Region functions --
----------------------

-- get a region
function getRegion(number)
	return region[number]
end

-- check for a win in a particular region.
-- returns a number representation of the region. occurrences of player 1
-- add 1, occurrences of player 2 subtract 1. so if there are two X pieces,
-- it will return 2. one O will return -1.
function checkWinInRegion(number)
	local to_return = 0
	for i, v in pairs(getRegion(number)) do
		local piece = getPiece(v["x"], v["y"])
		if piece == PLAYER_1 then to_return = to_return + 1 end
		if piece == PLAYER_2 then to_return = to_return - 1 end
	end
	return to_return
end

-- check for a win in every region.
-- returns false if no winner.
-- returns the winner if there is one.
function checkWin()
	for i in pairs(region) do
		local win = checkWinInRegion(i)
		if math.abs(win) == BOARD_RANK then
			if win == math.abs(win) then
				return PLAYER_1
			else
				return PLAYER_2
			end
		end
	end
	return false
end

------------------
-- UI Functions --
------------------
function stringisempty(s)
  return s == nil or s == ''
end
-- human play
function humanPlay(piece)
	
	print(piece .. ", here's the board:")
	displayBoard()
	local placed = false
	while placed == false do	-- loop until they play correctly
		print()
		print("Where would you like to play your " .. piece .. "?")
		term.write("X-coordinate (starting with 0): ")
		local x = tonumber(term.read())
		term.write("Now give the Y-coordinate (starting with 0): ")
		local y = tonumber(term.read())
		placed = placePiece(x, y, piece)
		if placed == false then
			term.write("You can't play there!")
		end
	end
	displayBoard()
	print()
	
end

-- AI play
function AIPlay(piece)
	
	-- am I negative or positive?
	local me = 0
	if piece == PLAYER_1 then me = 1 end
	if piece == PLAYER_2 then me = -1 end
	
	-- look for a region in which I can win
	for i in pairs(region) do
		local win = checkWinInRegion(i)
		if win == ((BOARD_RANK - 1) * me) then
			for j, v in pairs(getRegion(i)) do
				if isEmpty(v["x"], v["y"]) == true then
					placePiece(v["x"], v["y"], piece)
					return
				end
			end
		end
	end
	
	-- look for a region in which I can block
	for i in pairs(region) do
		local win = checkWinInRegion(i)
		if win == ((BOARD_RANK - 1) * (me * -1)) then
			for j, v in pairs(getRegion(i)) do
				if isEmpty(v["x"], v["y"]) == true then
					placePiece(v["x"], v["y"], piece)
					return
				end
			end
		end
	end
	
	-- play first empty space, if no better option
	for i = 0, (BOARD_RANK - 1) do
		for j = 0, (BOARD_RANK - 1) do
			if placePiece(i, j, piece) ~= false then return end
		end
	end
	
end

----------
-- Main --
----------

-- welcome!
print("Welcome to Tic-Tac-Toe!")
print()
print("1. Player VS CPU")
print("2. Player VS Player")
print("3. CPU VS CPU")
term.write(": ")
local plcpu = tonumber(term.read()) -- Are they playable by human or computer-controlled?
if plcpu == 1 then 
	PLAYER_1_HUMAN = true
	PLAYER_2_HUMAN = false
elseif plcpu == 2 then
	PLAYER_1_HUMAN = true
	PLAYER_2_HUMAN = true
elseif plcpu == 3 then
	PLAYER_1_HUMAN = false
	PLAYER_2_HUMAN = false
else print("Invalid option. Exiting")
return
end

print("Please insert a board size. Default is 3, Maximum is 16")
term.write("(3): ")
BOARD_RANK = tonumber(term.read())
if stringisempty(BOARD_RANK) then
BOARD_RANK = 3	-- The board will be this in both dimensions.
end
-------------------------------------------------------
-- Don't run if the board is larger than the maximum --
-------------------------------------------------------

if BOARD_RANK > MAX_BOARD_RANK then print("The board is too big. Exiting...") return end

-----------------------------
-- Create board (2D table) --
-----------------------------

space = {}
for i = 0, (BOARD_RANK - 1) do
	space[i] = {}
	for j = 0, (BOARD_RANK - 1) do
		space[i][j] = nil	-- start each space with nil
	end
end

-- play the game until someone wins
while true do
	
	-- break if the game is won
	if isGameOver() == true then break end
	
	-- player 1
	if PLAYER_1_HUMAN == true then humanPlay(PLAYER_1)
	else AIPlay(PLAYER_1) end
	term.clear()
	term.setPos(0,0)
	-- break if the game is won
	if isGameOver() == true then break end
	
	-- player 2
	if PLAYER_2_HUMAN == true then humanPlay(PLAYER_2)
	else AIPlay(PLAYER_2) end
	term.clear()
	term.setPos(0,0)
	
end

-- show the final board
print("The final board:")
displayBoard()
print()

-- write who won, or if there is a tie
win = checkWin()
if win == false then
	print("Tie game!")
else
	print(win .. " wins!")
end