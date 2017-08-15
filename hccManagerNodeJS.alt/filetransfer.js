'use strict';
var multipart = require("multipart");
var http = require('http'), path = require("path"), fs = require("fs");
class upload {
    uploadFile(request, response, url, basedir) {
        var fileName = basedir + url;
        this.upload_file(request, response, fileName);
    }
    /*
    * Display upload form
    */
    display_form(req, res) {
        res.writeHead(200, { "Content-Type": "text/html" });
        res.write('<form action="/upload" method="post" enctype="multipart/form-data">' +
            '<input type="file" name="upload-file">' +
            '<input type="submit" value="Upload">' +
            '</form>');
        res.end();
    }
    /*
 * Create multipart parser to parse given request
 */
    parse_multipart(req) {
        var parser = multipart.parser();
        // Make parser use parsed request headers
        parser.headers = req.headers;
        // Add listeners to request, transfering data to parser
        req.addListener("data", function (chunk) {
            parser.write(chunk);
        });
        req.addListener("end", function () {
            parser.end();
        });
        return parser;
    }
    /*
     * Handle file upload
     */
    upload_file(req, res, fileName) {
        // Request body is binary
        req.setBodyEncoding("binary");
        // Handle request as multipart
        var stream = this.parse_multipart(req);
        // var fileName = null;
        var fileStream = null;
        // Set handler for a request part received
        stream.onPartBegin = function (part) {
            console.log("Started part, name = " + part.name + ", filename = " + part.filename);
            // Construct file name
            // fileName = "./uploads/" + stream.part.filename;
            // Construct stream used to write to file
            fileStream = fs.createWriteStream(fileName);
            // Add error handler
            fileStream.addListener("error", function (err) {
                console.log("Got error while writing to file '" + fileName + "': ", err);
            });
            // Add drain (all queued data written) handler to resume receiving request data
            fileStream.addListener("drain", function () {
                req.resume();
            });
        };
        // Set handler for a request part body chunk received
        stream.onData = function (chunk) {
            // Pause receiving request data (until current chunk is written)
            req.pause();
            // Write chunk to file
            // Note that it is important to write in binary mode
            // Otherwise UTF-8 characters are interpreted
            console.log("Writing chunk");
            fileStream.write(chunk, "binary");
        };
        // Set handler for request completed
        stream.onEnd = function () {
            // As this is after request completed, all writes should have been queued by now
            // So following callback will be executed after all the data is written out
            fileStream.addListener("drain", function () {
                // Close file stream
                fileStream.end();
                // Handle request completion, as all chunks were already written
                this.upload_complete(res);
            });
        };
    }
    upload_complete(res) {
        console.log("Request complete");
        // Render response
        res.writeHead(200, { "Content-Type": "text/plain" });
        res.write("Thanks for playing!");
        res.end();
        console.log("\n=> Done");
    }
}
exports.upload = upload;
//# sourceMappingURL=filetransfer.js.map