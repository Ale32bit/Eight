local fs = require("filesystem")

function fs.combine(base, dir)

end

function fs.readFile(path, binary)
    local f = fs.open(path, binary and "rb" or "r");
    local content = f.readAll();
    f.close();
    return content;
end

function fs.writeFile(path, content, binary)
    local f = fs.open(path, binary and "wb" or "w");
    f.write(content);
    f.close();
end

function fs.appendFile(path, content, binary)
    local f = fs.open(path, binary and "ab" or "a");
    f.write(content);
    f.close();
end
