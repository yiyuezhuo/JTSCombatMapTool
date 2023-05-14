// https://habr.com/en/articles/684936/

// Assets/Plugins/WebGL/JSFileUploader.jslib
mergeInto(LibraryManager.library,
    {
        InitFileLoader: function (callbackObjectName, callbackMethodName) {
						// Strings received from C# must be decoded from UTF8
            FileCallbackObjectName = UTF8ToString(callbackObjectName);
            FileCallbackMethodName = UTF8ToString(callbackMethodName);
						
          	// Create an input to take files if there isn't one already
            var fileuploader = document.getElementById('fileuploader');
            if (!fileuploader) {
                console.log('Creating fileuploader...');
                fileuploader = document.createElement('input');
                fileuploader.setAttribute('style', 'display:none;');
                fileuploader.setAttribute('type', 'file');
                fileuploader.setAttribute('id', 'fileuploader');
                fileuploader.setAttribute('class', 'nonfocused');
                document.getElementsByTagName('body')[0].appendChild(fileuploader);

                fileuploader.onchange = function (e) {
                    var files = e.target.files;
										
                  	// If the file is not selected, we complete the execution and call unfocus
                  	// Note: If you need to handle the case where the file is not
                  	// selected, then you can call SendMessage and pass
                  	// null, instead ResetFileLoader()
										if (files.length === 0) {
                        ResetFileLoader();
                        return;
                    }
                  
                    console.log('ObjectName: ' + FileCallbackObjectName + ';\nMethodName: ' + FileCallbackMethodName + ';');
                    SendMessage(FileCallbackObjectName, FileCallbackMethodName, URL.createObjectURL(files[0]));
                };
            }

            console.log('FileLoader initialized!');
        },


				// This function is called when the button is pressed, because browser protection doesn't skip click() call
  			// programmatically
        RequestUserFile: function (extensions) {
          	// Decoding the string from UTF 8
            var str = UTF8ToString(extensions);
            var fileuploader = document.getElementById('fileuploader');
						
          	// If for some reason the fileuploader does not exist - set it
          	// This can happen in Blazor.NET projects
            if (fileuploader === null)
                InitFileLoader(FileCallbackObjectName, FileCallbackMethodName);
						
          	// Set the received extensions
            if (str !== null || str.match(/^ *$/) === null)
                fileuploader.setAttribute('accept', str);
						
          	// Focus on input and click
            fileuploader.setAttribute('class', 'focused');
            fileuploader.click();
        },
				
  			// This function is called after the file is received.
  			// It can be called from RequestUserFile or fileUploader.onchange
  			// not from C#, which will be faster, but I'm using the call from C# as a mini-example
  			// of calling a function without parameters
        ResetFileLoader: function () {
            var fileuploader = document.getElementById('fileuploader');

            if (fileuploader) {
              	// Removing input from focus
                fileuploader.setAttribute('class', 'nonfocused');
            }
        },
    });