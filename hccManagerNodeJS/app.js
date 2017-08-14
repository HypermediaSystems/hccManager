var http = require('http'), path = require("path"), fs = require("fs"), port = process.argv[2] || 3000, url = process.argv[3] || '', basedir = process.argv[4] || '';
var glob = require("glob");
// Colors for CLI output
var WHT = '\033[39m';
var RED = '\033[91m';
var GRN = '\033[32m';
if (url == '') {
    console.log("ERROR: no url given.");
    console.log("CALL: node app.js port url basedir");
}
else if (basedir == '') {
    console.log("ERROR: no basedir given.");
    console.log("CALL: node app.js port url basedir");
}
else {
    basedir = basedir.replace(/\\/g, "/");
    var requestHandler = function (request, response) {
        console.log(request.url);
        var url = request.url;
        if (url.indexOf("/entry/") == 0) {
            // /entry/test?url=fname
            getEntry(response, url.substring(7).replace("?url=", "/"));
        }
        else if (url === "/list") {
            getList(response);
        }
        else if (url === "/external") {
            getExternal(response);
        }
        else if (url.indexOf("/sites/") == 0) {
            getSite(response, url.substring(7));
        }
        else if (url.indexOf("/config/") == 0) {
            getSite(response, url.substring(8) + "/.hccConfig.json");
        }
        else {
            response.writeHead(404, { "Content-Type": "text/plain" });
            response.write("404 Not Found\n");
            response.end();
        }
    };
    var server = http.createServer(requestHandler);
    server.listen(port, function (err) {
        if (err) {
            return console.log('something bad happened', err);
        }
        console.log("server is listening on " + port);
    });
}
function getList(response) {
    var read = function (dir) {
        return fs.readdirSync(dir)
            .reduce(function (files, file) {
            return fs.statSync(path.join(dir, file)).isDirectory() ?
                files.concat(read(path.join(dir, file))) :
                files.concat(path.join(dir, file));
        }, []);
    };
    glob(basedir + "**/*.*", function (er, files) {
        // files is an array of filenames.
        // If the `nonull` option is set, and nothing
        // was found, then files is ["**/*.js"]
        // er is an error object or null.
        var list = [];
        files.forEach(function (value) {
            var fn = value.substr(basedir.length);
            var e = new entry();
            e.fname = fn;
            e.url = url + '/' + fn;
            list.push(e);
        });
        response.end(JSON.stringify(list));
        // response.end("get list for " + url + " in " + basedir);
    });
}
function getEntry(response, url) {
    // response.end("get entry for " + url);
    sendFile(response, basedir + url, false);
}
function sendFile(response, filename, fileNotFound) {
    // Setting up MIME-Type (YOU MAY NEED TO ADD MORE HERE) <--------
    var contentTypesByExtension = {
        '.html': 'text/html',
        '.css': 'text/css',
        '.js': 'text/javascript',
        '.json': 'text/json',
        '.svg': 'image/svg+xml'
    };
    var fileNotFound = false;
    // Assuming the file exists, read it
    fs.readFile(filename, 'binary', function (err, file) {
        // Output a green line to console explaining the file that will be loaded in the browser
        console.log(GRN + 'FILE: ' + WHT + filename);
        // If there was an error trying to read the file
        if (err) {
            // Put the error in the browser
            response.writeHead(500, { 'Content-Type': 'text/plain' });
            response.write(err + '\n');
            response.end();
            return;
        }
        if (fileNotFound === true) {
            response.writeHead(404, { 'Content-Type': 'text/plain' });
            response.write('Not found ' + filename + '\n');
            response.end();
            return;
        }
        // Otherwise, declar a headers object and a var for the MIME-Type
        var headers = {};
        var contentType = contentTypesByExtension[path.extname(filename)];
        // If the requested file has a matching MIME-Type
        if (contentType) {
            // Set it in the headers
            headers['Content-Type'] = contentType;
        }
        // Output the read file to the browser for it to load
        response.writeHead(200, headers);
        response.write(file, 'binary');
        response.end();
    });
}
function getSite(response, url) {
    // Setting up MIME-Type (YOU MAY NEED TO ADD MORE HERE) <--------
    var contentTypesByExtension = {
        '.html': 'text/html',
        '.css': 'text/css',
        '.js': 'text/javascript',
        '.json': 'text/json',
        '.svg': 'image/svg+xml'
    };
    var filename = basedir + url;
    if (url.indexOf(".") < 0) {
        filename = basedir;
    }
    var fileNotFound = false;
    // Check if the requested file exists
    fs.exists(filename, function (exists) {
        // If it doesn't
        if (!exists) {
            // Output a red error pointing to failed request
            console.log(RED + 'FAIL: ' + filename);
            // Redirect the browser to the 404 page
            filename = path.join(basedir, '404.html');
            fileNotFound = true;
        }
        else if (fs.statSync(filename).isDirectory()) {
            // Output a green line to the console explaining what folder was requested
            console.log(GRN + 'FLDR: ' + WHT + filename);
            // redirect the user to the index.html in the requested folder
            filename = basedir + 'index.html';
        }
        // Assuming the file exists, read it
        fs.readFile(filename, 'binary', function (err, file) {
            // Output a green line to console explaining the file that will be loaded in the browser
            console.log(GRN + 'FILE: ' + WHT + filename);
            // If there was an error trying to read the file
            if (err) {
                // Put the error in the browser
                response.writeHead(500, { 'Content-Type': 'text/plain' });
                response.write(err + '\n');
                response.end();
                return;
            }
            if (fileNotFound === true) {
                response.writeHead(404, { 'Content-Type': 'text/plain' });
                response.write('Not found ' + filename + '\n');
                response.end();
                return;
            }
            // Otherwise, declar a headers object and a var for the MIME-Type
            var headers = {};
            var contentType = contentTypesByExtension[path.extname(filename)];
            // If the requested file has a matching MIME-Type
            if (contentType) {
                // Set it in the headers
                headers['Content-Type'] = contentType;
            }
            // Output the read file to the browser for it to load
            response.writeHead(200, headers);
            response.write(file, 'binary');
            response.end();
        });
    });
}
function getExternal(response) {
    var sList = "abc";
    var zStart = 13;
    var zCnt = 3;
    var xStart = 4281;
    var xCnt = 4;
    var yStart = 2705;
    var yCnt = 4;
    // 13/51.9639/8.2343
    // https://b.tile.openstreetmap.org/13/4282/2706.png
    var urlPattern = "http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png";
    var list = [];
    for (var z = zStart; z < zStart + zCnt; z++) {
        for (var y = yStart; y < yStart + yCnt; y++) {
            for (var x = xStart; x < xStart + xCnt; x++) {
                var url = urlPattern.replace("{s}", sList[0])
                    .replace("{z}", z.toString())
                    .replace("{y}", y.toString())
                    .replace("{x}", x.toString());
                console.log("get: " + url);
                var e = new entry();
                e.url = url;
                e.needReplace = false;
                e.canBeZipped = false;
                list.push(e);
                for (var i = 1; i < sList.length; i++) {
                    var aliasUrl = urlPattern.replace("{s}", sList[i])
                        .replace("{z}", z.toString())
                        .replace("{y}", y.toString())
                        .replace("{x}", x.toString());
                    console.log("alias: " + url + " -> " + aliasUrl);
                    var e = new entry();
                    e.url = url;
                    e.aliasUrl = aliasUrl;
                    e.needReplace = false;
                    e.canBeZipped = false;
                    list.push(e);
                }
            }
        }
        yStart *= 2;
        xStart *= 2;
        yCnt *= 2;
        xCnt *= 2;
    }
    response.end(JSON.stringify(list));
}
var entry = (function () {
    function entry() {
        this.canBeZipped = true;
        this.needReplace = true;
    }
    return entry;
}());
//# sourceMappingURL=app.js.map