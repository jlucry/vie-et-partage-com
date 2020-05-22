// Services definition.
(function () {
    'use strict';

    angular
        .module('app.posts.service')
        .factory('post', post);

    post.$inject = ['$rootScope', '$http', '$log', 'Upload', 'regions'];

    // A RESTful factory for retrieving post
    function post($rootScope, $http, $log, Upload, regions) {
        var data = null;
        var filesToPost = null;
        var filesToPostIndex = 0;
        var filesToPostError = 0;
        var filesToPostDenied = false;
        var filesToRemove = null;
        var filesToRemoveIndex = 0;
        var filesToRemoveError = 0;
        var service = {
            data: data,

            get: get,
            post: post,
            postFile: postFile,
            removeFile: removeFile,
            onLoading: null,
            onLoaded: null,
            //getContain: getContain,
            //getAddress: getAddress,
            //getRegistration: getRegistration,
            //getAuthor: getAuthor,
            //getEditor: getEditor,
        };
        return service;

        //TODO: A Optimiser: retrouver toutes les valeurs expose dans getComplete avec une seul for.

        ///////////////////////

        function get(id) {
            service.data = null;

            if (service.onLoading != null) {
                service.onLoading(false);
            }

            // Querying...
            $log.debug('Getting post...');
            var path = '/api/post/' + id;
            $log.debug('Getting post with ', path);

            return $http.get(path)
                .then(getComplete)
                .catch(getFailed);

            function getComplete(response) {
                $log.debug('Post got.');
                service.data = response.data;
                $log.debug('data=', service.data);
                if (service.onLoaded != null) {
                    service.onLoaded(false);
                }
                return service;
            }

            function getFailed(error) {
                $log.error('Faile to get post: ' + error.data);
                if (service.onLoaded != null) {
                    service.onLoaded(false);
                }
            }
        }

        function post(type) {
            if (service.onLoading != null) {
                service.onLoading(true);
            }

            // Querying...
            $log.debug('Update post...');
            var path = '/api/post?region=' + regions.getCurrentName() + '&type=' + type;
            $log.debug('Update', service.data, 'to', path);

            return $http.post(path, service.data)
                .then(getComplete)
                .catch(getFailed);

            function getComplete(response) {
                $log.debug('Post updated, response=', response);
                if (response.data == null || response.data == "") {
                    if (service.onError != null) {
                        service.onError(true);
                    }
                }
                else if (response.data.Error != null) {
                    service.data.Error = response.data.Error;
                    if (service.onError != null) {
                        service.onError(true);
                    }
                }
                else {
                    service.data = response.data;
                    //$log.debug('data=', service.data);
                    if (service.onLoaded != null) {
                        service.onLoaded(true);
                    }
                }
            }

            function getFailed(error) {
                $log.error('Failed to update post: ' + error.data);
                if (service.onError != null) {
                    service.onError(true);
                }
            }
        }

        function postFile(files) {
            if (files != null && files.length != 0) {
                filesToPost = files;
                filesToPostIndex = 0;
                filesToPostError = 0;
                filesToPostDenied = false;
                _postFile();
            }
        }

        function removeFile(fileIsd) {
            if (fileIsd != null && fileIsd.length != 0) {
                filesToRemove = fileIsd;
                filesToRemoveIndex = 0;
                filesToRemoveError = 0;
                _removeFile();
            }
        }

        ///////////////////////

        function _postFile() {
            if (filesToPostIndex == 0) {
                if (service.onLoading != null) {
                    service.onLoading(true);
                }
            }
            var file = filesToPost[filesToPostIndex].file;
            var type = filesToPost[filesToPostIndex].type;

            $log.debug('Upload file' + filesToPostIndex + ':', filesToPost[filesToPostIndex], '...');
            file.errorMsg = null;
            file.upload = Upload.http({
                url: '/file/Upload?id=' + service.data.Id + '&name=' + file.name + "&type=" + type,
                data: file
            });

            file.upload.then(function (response) {
                $log.debug('File ' + file.name + ' uploaded, response=', response);
                file.result = response.data;
                // Cover case...
                if (type == 1) {
                    // Update the cover...
                    service.data.Cover = response.data;
                    $log.debug('Cover uploaded to:', service.data.Cover);
                }
                else if (type == 2) {
                    // Update the croped cover...
                    service.data.CoverCrop = response.data;
                    $log.debug('Cropped cover uploaded to:', service.data.CoverCrop);
                }
                else {
                    $log.debug('File uploaded to:', response.data);
                }
                // Process the next file...
                _postFileNext(response.data);
            }, function (response) {
                if (response.status > 0) {
                    $log.error('Failed to upload file', file.name, response.status, response.data);
                    file.errorMsg = response.status + ': ' + response.data;
                    // Process the next file...
                    _postFileNext(response.data);
                }
            });

            file.upload.progress(function (evt) {
                file.progress = Math.min(100, parseInt(100.0 * evt.loaded / evt.total));
                $log.debug('File  ' + file.name + 'uploadprogress=', file.progress);
            });
        };

        function _postFileNext(response) {
            if (response.startsWith('KO') == true) {
                filesToPostError += 1;
                //$log.debug('Check for Accès refusé in:', response);
                if (response.indexOf("refus") != -1) {
                    //$log.debug('Accès refusé detecté!!!');
                    filesToPostDenied = true;
                }
            }
            if (filesToPostIndex != (filesToPost.length - 1)) {
                // Move to next file...
                filesToPostIndex += 1;
                _postFile();
            }
            else {
                // All files has been sent, return the upload result....
                if (filesToPostError == 0 && service.onLoaded != null) {
                    service.onLoaded(true);
                }
                else if (service.onError != null) {
                    if (filesToPostDenied == true)
                        service.data.Error = "Acc&egrave;s refus&eacute;!";
                    service.onError(true);
                }
            }
        }

        function _removeFile() {
            if (filesToRemoveIndex == 0) {
                if (service.onLoading != null) {
                    service.onLoading(true);
                }
            }
            var fileId = filesToRemove[filesToRemoveIndex];

            // Querying...
            var path = '/api/post/' + service.data.Id + "/" + fileId;
            $log.debug('Deleting file with ', path, "...");

            return $http.delete(path)
                .then(getComplete)
                .catch(getFailed);

            function getComplete(response) {
                $log.debug('data=', service.data);
                // Cover case...
                if (fileId == 0) {
                    // Update the covers...
                    service.data.Cover = '';
                    service.data.CoverCrop = '';
                    $log.debug('Cover deleted.');
                }
                else {
                    $log.debug('File ' + fileId + ' deleted.');
                }
                // Process the next file...
                _removeFileNext(response.data);
            }

            function getFailed(response) {
                if (response.status > 0) {
                    $log.error('Failed to delete file', fileId, response.status, response.data);
                    // Process the next file...
                    _removeFileNext(response.data);
                }
            }
        }

        function _removeFileNext(response) {
            $log.debug('_removeFileNext:', response);
            if (response == false) {
                filesToRemoveError += 1;
            }
            if (filesToRemoveIndex != (filesToRemove.length - 1)) {
                // Move to next file...
                filesToRemoveIndex += 1;
                _removeFile();
            }
            else {
                // All files has been sent, return the upload result....
                if (filesToRemoveError == 0 && service.onLoaded != null) {
                    service.onLoaded(true);
                }
                else if (service.onError != null) {
                    service.onError(true);
                }
            }
        }

        //function getContain() {
        //    return getText('contain');
        //}
        //function getAddress() {
        //    return getText('address');
        //}

        //function getRegistration() {
        //    return getClaim('registration');
        //}
        //function getAuthor() {
        //    return getClaim('author');
        //}
        //function getEditor() {
        //    return getClaim('editor');
        //}

        ///////////////////////

        //function getClaim(type) {
        //    if (type != null && service.data != null && service.data.PostClaims != null) {
        //        for (var i = 0; i < service.data.PostClaims.length; i++) {
        //            //$log.debug('-----Search', service.data.PostClaims[i]);
        //            if (service.data.PostClaims[i].Type == type) {
        //                //$log.debug('+++++found', service.data.PostClaims[i].Value);
        //                return service.data.PostClaims[i].StringValue;
        //            }
        //        }
        //    }
        //    return null;
        //}

        //function getText(type) {
        //    if (type != null && service.data != null && service.data.PostTexts != null) {
        //        for (var i = 0; i < service.data.PostTexts.length; i++) {
        //            //$log.debug('-----Search', service.data.PostTexts[i]);
        //            if (service.data.PostTexts[i].Type == type) {
        //                //$log.debug('+++++found', service.data.PostTexts[i].Value);
        //                return service.data.PostTexts[i].Value;
        //            }
        //        }
        //    }
        //    return null;
        //}
    };
})();