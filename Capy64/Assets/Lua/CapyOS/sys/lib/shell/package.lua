local expect = require("expect").expect
local fs = require("fs")
local nativePackage = package

local function copyTable(source, target)
    target = target or {}

    for k, v in pairs(source) do
        target[k] = v
    end

    return target
end

local hostPackages = copyTable(nativePackage._host)

local function createPreloadSearcher(envPackage)
    return function(name)
        if not envPackage.preload[name] then
            return string.format("no field package.preload['%s']", name)
        end
        return envPackage.preload[name], ":preload:"
    end
end

local function createLoaderSearcher(envPackage)
    return function(name)
        local path, err = envPackage.searchpath(name, envPackage.path)

        if not path then
            return err
        end

        local func, err = loadfile(path)
        if not func then
            return string.format("error loading module '%s' from file '%s':\t%s", name, path, err)
        end

        return func, path
    end
end

local function createEnvironment(filePath)
    local envPackage = {
        cpath = nativePackage.cpath,
        searchpath = nativePackage.searchpath,
        config = nativePackage.config,
        searchers = {},
        loaded = {},
        preload = {},
    }

    local dirName = fs.getDir(filePath)
    --envPackage.path = string.format("%s/?.lua;%s/?/init.lua;", dirName, dirName) .. nativePackage.path
    envPackage.path = nativePackage.path

    envPackage.searchers[1] = createPreloadSearcher(envPackage)
    envPackage.searchers[2] = createLoaderSearcher(envPackage)

    local function envRequire(modname)
        expect(1, modname, "string", "number")
        modname = tostring(modname)
    
        if envPackage.loaded[modname] then
            return envPackage.loaded[modname]
        end
    
        local errorOutput = ""
        local libFunction, libPath
        for i = 1, #envPackage.searchers do
            local par, path = envPackage.searchers[i](modname)
            if type(par) == "function" then
                libFunction, libPath = par, path
                break
            else
                errorOutput = errorOutput .. "\n\t" .. par
            end
        end
    
        if not libFunction then
            error(string.format("module '%s' not found:%s", modname, errorOutput), 2)
        end
    
        local ok, par = pcall(libFunction)
        if not ok then
            error(par, 0)
        end
    
        if par == nil then
            envPackage.loaded[modname] = true
            return true
        end
    
        envPackage.loaded[modname] = par
    
        return par, libPath
    end

    copyTable(hostPackages, envPackage.loaded)
    envPackage.loaded.package = envPackage

    local env_G = copyTable(envPackage.loaded._G or _G)
    envPackage.loaded._G = env_G
    env_G._G = env_G

    envPackage.loaded._G.package = envPackage
    envPackage.loaded._G.require = envRequire

    return envPackage, envRequire
end

return createEnvironment
