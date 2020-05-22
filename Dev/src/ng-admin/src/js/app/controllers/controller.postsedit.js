(function () {
    'use strict';

    // Posts controllers.
    angular
        .module('app.posts')
        .controller('PostEditController', PostEditController);

    // Post edition controller.
    PostEditController.$inject = ['$rootScope', '$scope', '$state', '$timeout', '$location', '$log', '$mdSidenav', '$sce', 'Upload', 'heads', 'siteConf', 'post'];

    function PostEditController($rootScope, $scope, $state, $timeout, $location, $log, $mdSidenav, $sce, Upload, heads, siteConf, post) {
        $log.debug("PostEditController...");

        var applyReturnToView = false;
        var coverVisible = "visibility:visible;";
        var coverHidden = "visibility:hidden;";
        /*jshint validthis: true */
        var edt = this;
        edt.Post = null;
        edt.PostCover = null;
        edt.PostCoverCrop = null;
        edt.PostImg = false;
        edt.PostMedia = false;
        edt.PostMediaVideo = false;
        edt.PostFiles = false;
        edt.LoadClass = 'ng-hide';
        edt.ShowProgress = false;
        edt.Edition = 0;
        edt.CoverEditionStyle = coverHidden;
        edt.Pushing = false;
        edt.Error = null;
        edt.Pub = false;

        edt.EditText = editText;
        edt.EditDate = editDate;
        edt.EditSettings = editSettings;
        edt.EditFile = editFile;
        edt.EditCover = editCover;
        edt.Apply = apply;
        edt.Remove = remove;
        edt.Close = close;
        edt.Cancel = cancel;
        edt.CatMarginLeft = catMarginLeft;
        edt.ReloadList = 0;

        edt.CoverProgress = 0;
        edt.CoverResult = null;
        edt.CoverErrorMsg = null;

        edt.TrustedTextContain = null;

        ///////////////////////

        edt.SummerOptions = {
            toolbar: [
                ['option1', ['fullscreen', 'codeview', 'undo', 'redo']],
                ['style', ['style', 'bold', 'italic', 'underline', 'clear']],
                ['para', ['ul', 'ol', 'paragraph', 'hr']],
                ['table', [ 'table', ]],
                ['insert', [ 'link', 'picture', 'video' ]]                
            ],
            lang: 'fr-FR',
            minHeight: null,             // set minimum height of editor
            maxHeight: null,             // set maximum height of editor
            dialogsInBody: true
        };

        ///////////////////////
        
        /*var handleFileSelect = function (evt) {
            $log.debug('@@@@@@@@@@@@@handleFileSelect: evt=', evt);
            var file = evt.currentTarget.files[0];
            var reader = new FileReader();
            reader.onload = function (evt) {
                $scope.$apply(function ($scope) {
                    $scope.myImage = evt.target.result;
                    //$log.debug('@@@@@@@@@@@@@handleFileSelect:onload: myImage=', $scope.myImage);
                });
            };
            //onerror
            reader.readAsDataURL(file);
        };
        angular.element(document.querySelector('#fileInputCrop')).on('change', handleFileSelect);*/

        ///////////////////////

        post.onLoading = onLoading;
        post.onLoaded = onLoaded;
        post.onError = onError;

        ///////////////////////

        function onLoading(edit) {
            if (edit == false) {
                $log.debug('OnLoading (edit=false)...');
                $log.debug('edt.Edition=', edt.Edition);
                if (edt.Edition == 4 && applyReturnToView == false) {
                }
                else {
                    _clean();
                }
            }
            else {
                $log.debug('OnLoading (edit=true)...');
                edt.LoadClass = 'empty';
                edt.ShowProgress = true;
                edt.Pushing = true;
                edt.Error = null;
            }
        }

        function onLoaded(edit) {
            $log.debug('OnLoaded (edit = false)...');
            $log.debug('edt.Edition=', edt.Edition);
            // Case where we need to reload the post just after the edition...
            if (edit == true && edt.Edition == 4) {
                if (applyReturnToView == true) {
                    _reload();
                }
                else {
                    $log.debug('Reload post to have the updated file list...');
                    edt.files = [];
                    post.get(edt.Post.Id);
                }
                return;
            }

            // Stop progress bar...
            $timeout(function () {
                edt.LoadClass = 'ng-hide';
                edt.ShowProgress = false;
                edt.Pushing = false;
                if (applyReturnToView == true && edit == true) {
                    // Return to post view...
                    $log.debug('Stop edition...');
                    edt.Edition = 0;
                }
            }, 100);

            // Update the post...
            edt.Post = post.data;
            if (edt.Post.Id == 0) {
                // Case of post adding, switch to edition...
                editText();
            }
            else {
                // Collect post files...
                edt.PostImg = (edt.Post.PostImg != null && edt.Post.PostImg.length != 0) ? true : false;
                edt.PostMedia = (edt.Post.PostMedia != null && edt.Post.PostMedia.length != 0) ? true : false;
                edt.PostMediaVideo = (edt.Post.PostMediaVideo != null && edt.Post.PostMediaVideo.length != 0) ? true : false;
                edt.PostFiles = (edt.Post.PostFiles != null && edt.Post.PostFiles.length != 0) ? true : false;
                edt.Pub = (edt.Post.State == 1) ? true : false;
            }

            // Save that we need to reload the post list...
            if (edit == true) { edt.ReloadList = 1; }

            // Cover related...
            if (edt.Edition == 5) {
                // Reset cover form...
                edt.picFile = null;
                edt.croppedDataUrl = null;
            }
            // Always force cover reload...
            edt.PostCover = (edt.Post.Cover == null || edt.Post.Cover == '') ? '' : edt.Post.Cover + "?ts=" + Date.now();;
            edt.PostCoverCrop = (edt.Post.Cover == null || edt.Post.Cover == '') ? '' : edt.Post.CoverCrop + "?ts=" + Date.now();;

            // Trust the text...
            edt.TrustedTextContain = $sce.trustAsHtml(edt.Post.TextContain);
            $log.debug('TrustedTextContain=', edt.TrustedTextContain);

            // Init registration form...
            if (edt.Post != null && edt.Post.ClaimRegistration == true) {
                enableEvent();
            }
        }

        function onError(edit, errorMsg) {
            $log.debug('Post load failed...');
            $timeout(function () {
                edt.LoadClass = 'ng-hide';
                edt.ShowProgress = false;
                edt.Pushing = false;
            }, 100);
            $log.debug('post.data.Error=', post.data.Error);
            edt.Error = (post.data.Error != null)
                ? post.data.Error
                : "Une erreur s'est produite veuillez r&eacute;essayer ult&eacute;rieurement!";
            post.data.Error = null;
            // Save that we need to reload the post list...
            if (edit == true) { edt.ReloadList = 1; }
        }

        ///////////////////////

        function catMarginLeft(cat) {
            if (cat != null) {
                return (cat.Deep * 20) + "px";
            }
            return "0px"
        }

        ///////////////////////

        function editText() {
            edt.Edition = 1;
            edt.CoverEditionStyle = coverHidden;
        }

        function editDate() {
            edt.Edition = 2;
            edt.CoverEditionStyle = coverHidden;
        }

        function editSettings() {
            edt.Edition = 3;
            edt.CoverEditionStyle = coverHidden;
        }

        function editFile() {
            edt.Edition = 4;
            edt.CoverEditionStyle = coverHidden;
        }

        function editCover() {
            edt.Edition = 5;
            edt.CoverEditionStyle = coverVisible;
        }

        ///////////////////////

        function apply() {
            $log.debug('PostEditController: apply - edt.Edition=', edt.Edition);
            if (edt.Edition == 1)
                applyText();
            else if (edt.Edition == 2)
                applyDate();
            else if (edt.Edition == 3)
                applySettings();
            else if (edt.Edition == 4)
                applyFile();
            else if (edt.Edition == 5)
                applyCover();
        }

        function applyText() {
            $log.debug('PostEditController: applyText');
            // Send post texts...
            post.post(1);
        }

        function applyDate() {
            $log.debug('PostEditController: applyDate');
            // Send post dates and registration...
            post.post(2);
        }

        function applySettings() {
            $log.debug('PostEditController: applySettings');
            // Change post state...
            edt.Post.State = (edt.Pub == true) ? 1 : 0;
            // Send post settings...
            post.post(3);
        }

        function applyFile() {
            $log.debug('PostEditController: applyFile');
            edt.CoverEditionStyle = coverHidden;
            edt.Progress = 0;

            // Upload files...
            if (edt.files != null) {
                var files = [];
                for (var i = 0; i < edt.files.length; i++) {
                    edt.files[i].progress = 0;
                    edt.files[i].error = 0;
                    files.push({
                        file: edt.files[i],
                        type: 0
                    });
                }
                $log.debug('PostEditController: applyCover: covers=', files);
                post.postFile(files);
            }
        }

        function applyCover() {
            $log.debug('PostEditController: applyCover (and cover)');
            //$log.debug('PostEditController: cover=', $scope.myCroppedImage);
            //$log.debug('PostEditController: applyCover (and cover)');

            // Upload cover...
            if (edt.picFile != null && edt.picFile.name != null && edt.croppedDataUrl != null) {
                var files = [];
                files.push({
                    file: edt.picFile,
                    type: 1
                });
                files.push({
                    file: Upload.dataUrltoBlob(edt.croppedDataUrl, edt.picFile.name),
                    type: 2
                });
                $log.debug('PostEditController: applyCover: covers=', files);
                post.postFile(files);
            }
        }

        ///////////////////////

        function remove() {
            if (edt.Edition == 1)
                removeText();
            else if (edt.Edition == 2)
                removeDate();
            else if (edt.Edition == 3)
                removeSettings();
            else if (edt.Edition == 4)
                removeFile();
            else if (edt.Edition == 5)
                removeCover();
        }

        function removeText() {
            $log.debug('PostEditController: removeText');
            //TODO
            apply();
        }

        function removeDate() {
            $log.debug('PostEditController: removeDate');
            edt.Post.StartDate = null;
            edt.Post.StartDateString = null;
            edt.Post.EndDate = null;
            edt.Post.EndDateString = null;
            // Send post dates and registration...
            post.post(2);
        }

        function removeSettings() {
            $log.debug('PostEditController: removeSettings');
            //TODO
            applySettings();
        }

        function removeFile() {
            $log.debug('PostEditController: removeFile');
            $log.debug('PostEditController: edt.Post.PostMediaVideo=', edt.Post.PostMediaVideo);
            $log.debug('PostEditController: edt.Post.PostMedia=', edt.Post.PostMedia);
            $log.debug('PostEditController: edt.Post.PostImg=', edt.Post.PostImg);
            $log.debug('PostEditController: edt.Post.PostFiles=', edt.Post.PostFiles);

            // Upload files...
            var fileIds = [];
            if (edt.Post.PostMediaVideo != null) { for (var i = 0; i < edt.Post.PostMediaVideo.length; i++) { if (edt.Post.PostMediaVideo[i].checked == true) fileIds.push(edt.Post.PostMediaVideo[i].Id); } }
            if (edt.Post.PostMedia != null) { for (var i = 0; i < edt.Post.PostMedia.length; i++) { if (edt.Post.PostMedia[i].checked == true) fileIds.push(edt.Post.PostMedia[i].Id); } }
            if (edt.Post.PostImg != null) { for (var i = 0; i < edt.Post.PostImg.length; i++) { if (edt.Post.PostImg[i].checked == true) fileIds.push(edt.Post.PostImg[i].Id); } }
            if (edt.Post.PostFiles != null) { for (var i = 0; i < edt.Post.PostFiles.length; i++) { if (edt.Post.PostFiles[i].checked == true) fileIds.push(edt.Post.PostFiles[i].Id); } }
            $log.debug('PostEditController: removeCover: covers=', fileIds);
            post.removeFile(fileIds);
        }

        function removeCover() {
            $log.debug('PostEditController: removeCover');
            // Remove cover...
            var fileIds = [];
            fileIds.push(0);
            $log.debug('PostEditController: removeCover: covers=', fileIds);
            post.removeFile(fileIds);
        }

        ///////////////////////

        function close() {
            $mdSidenav('PostEditSide').close()
                .then(function () {
                    PostShowActions();
                    _clean();
                    if (edt.ReloadList == 1) {
                        if (heads.Reload != null) {
                            heads.Reload(0);
                        }
                        edt.ReloadList = 0;
                    }
                });
        }

        function cancel() {
            if (edt.Post == null || edt.Post.Id == 0) {
                close();
                return;
            }
            _reload();
        }

        function resetEventField() {
            $log.debug('resetEventField...');
            if (edt.Post != null) {
                edt.Post.RegistrationFields = [];
                angular.copy(edt.Post.DefaultRegistrationFields, edt.Post.RegistrationFields);
                //edt.Post.RegistrationFields = edt.Post.DefaultRegistrationFields;
                $log.debug('resetEventField:edt.Post.RegistrationFields=', edt.Post.RegistrationFields);
            }
        }

        function enableEvent() {
            $log.debug('enableEvent...');
            if (edt.Post != null) {
                if (edt.Post.RegistrationFields == null) {
                    resetEventField();
                }
            }
        }

        function addEventField() {
            $log.debug('addEventField...');
            if (edt.Post != null) {
                if (edt.Post.RegistrationFields == null) {
                    edt.Post.RegistrationFields = [];
                }
                edt.Post.RegistrationFields.push({
                    Name: "",
                    Type: 1,
                    Choose: [],
                    Choose2: []
                });
                $log.debug('addEventField:edt.Post.RegistrationFields=', edt.Post.RegistrationFields);
            }
        }

        function moveEventField(direction, index) {
            $log.debug('moveEventField: direction=', direction, " and index=", index);
            if (edt.Post != null && edt.Post.RegistrationFields != null) {
                var max = edt.Post.RegistrationFields.length;
                if (index >= 0 && index <= (max - 1)) {
                    if (direction == 0) {
                        edt.Post.RegistrationFields.splice(index, 1);
                    }
                    else if (direction == 1 && index >= 1) {
                        // Move up...
                        var tmp = edt.Post.RegistrationFields[index];
                        edt.Post.RegistrationFields[index] = edt.Post.RegistrationFields[index - 1];
                        edt.Post.RegistrationFields[index - 1] = tmp;
                        $log.debug('moveEventField:Up->edt.Post.RegistrationFields=', edt.Post.RegistrationFields);
                    }
                    else if (direction == -1 && index < (max - 1)) {
                        // Move down...
                        var tmp = edt.Post.RegistrationFields[index];
                        edt.Post.RegistrationFields[index] = edt.Post.RegistrationFields[index + 1];
                        edt.Post.RegistrationFields[index + 1] = tmp;
                        $log.debug('moveEventField:Down->edt.Post.RegistrationFields=', edt.Post.RegistrationFields);
                    }
                }
                /*var tmp2 = edt.Post.RegistrationFields;
                //edt.Post.RegistrationFields = null;
                edt.Post.RegistrationFields = {};
                edt.Post.RegistrationFields = tmp2;*/
            }
        }  

        ///////////////////////

        function _clean() {
            $log.debug('Clean view\\edit post...');
            edt.Post = null;
            //edt.Title = 'Chargement...';
            //edt.PostText = null;
            //edt.PostAddress = null;
            //edt.PostRegistration = null;
            //edt.PostMediaAuthor = null;
            //edt.PostMediaEditor = null;
            edt.PostImg = false;
            edt.PostMedia = false;
            edt.PostMediaVideo = false;
            edt.PostFiles = false;
            edt.LoadClass = 'empty';
            edt.ShowProgress = true;
            edt.Edition = 0;
            edt.CoverEditionStyle = coverHidden;
            edt.files = null;
            //$scope.myImage = '';
            //$scope.myCroppedImage = '';
            edt.Error = null;
            edt.picFile = null;
            edt.croppedDataUrl = null;
            edt.CoverProgress = 0;
            edt.PostCover = null;
            edt.PostCoverCrop = null;
            edt.TrustedTextContain = null;
            edt.EnableEvent = enableEvent;
            edt.ResetEventField = resetEventField;
            edt.AddEventField = addEventField;
            edt.MoveEventField = moveEventField;
        }

        function _reload() {
            var pstId = 0;
            pstId = (edt.Post == null || edt.Post.Id == null) ? 0 : edt.Post.Id;
            _clean();
            if (pstId != 0) {
                post.get(pstId);
            }
        }

        ////////////////////////////////////////
        ////////////////////////////////////////
        ////////////////////////////////////////

        ////////////////////////////////////////

        //https://github.com/danialfarid/ng-file-upload/blob/master/demo/src/main/webapp/index.html
        //https://github.com/danialfarid/ng-file-upload/blob/master/demo/C%23/UploadHandler.ashx.cs

        ////////////////////////////////////////
        ////////////////////////////////////////
        ////////////////////////////////////////
        var version = '11.0.0';
        $scope.howToSend = 2;

        /*$scope.usingFlash = FileAPI && FileAPI.upload != null;
        //Upload.setDefaults({ngfKeep: true, ngfPattern:'image/*'});
        $scope.changeAngularVersion = function () {
            window.location.hash = $scope.angularVersion;
            window.location.reload(true);
        };
        $scope.angularVersion = window.location.hash.length > 1 ? (window.location.hash.indexOf('/') === 1 ?
          window.location.hash.substring(2) : window.location.hash.substring(1)) : '1.2.24';*/

        edt.files = [];

        edt.ProgressBarClass = function (file) {
            return (file.error != 0) ? "progress-bar-danger" : "";
        }
    
        //$scope.$watch('files', function (files) {
        //    $log.debug('$scope.$watch(files)...');
        //    $scope.formUpload = false;
        //    if (files != null) {
        //        if (!angular.isArray(files)) {
        //            $timeout(function () {
        //                $scope.files = files = [files];
        //            });
        //            return;
        //        }
        //        for (var i = 0; i < files.length; i++) {
        //            Upload.imageDimensions(files[i]).then(function (d) {
        //                $scope.d = d;
        //            });
        //            $scope.errorMsg = null;
        //            (function (f) {
        //                $scope.upload(f, true, 0);
        //            })(files[i]);
        //        }
        //    }
        //});
    
        $scope.upload = function (file, resumable, type) {
            $log.debug('$scope.upload...');
            $scope.errorMsg = null;
            if ($scope.howToSend === 1) {
                uploadUsingUpload(file, resumable, type);
            } else if ($scope.howToSend == 2) {
                uploadUsing$http(file, type);
            }
        };
    
        edt.isResumeSupported = Upload.isResumeSupported();
    
        $scope.restart = function (file) {
            $log.debug('$scope.restart...');
            file.progress = 0;
            file.error = 0;
            /*if (Upload.isResumeSupported()) {
                $http.get('https://angular-file-upload-cors-srv.appspot.com/upload?restart=true&name=' + encodeURIComponent(file.name)).then(function () {
                    $scope.upload(file, true);
                });
            } else*/ {
                $scope.upload(file);
            }
        };
    
        $scope.chunkSize = 100000;
        //$scope.chunkSize = 10000;
        function uploadUsingUpload(file, resumable, type) {
            $log.debug('uploadUsingUpload...');
            file.upload = Upload.upload({
                ///*url: 'Uploads/UploadHandler.ashx',
                //data: { name: user.Name },
                //file: file, // or list of files ($files) for html5 only*/
                //url: '/Upload', //'https://angular-file-upload-cors-srv.appspot.com/upload' + $scope.getReqParams(),
                ////resumeSizeUrl: resumable ? 'https://angular-file-upload-cors-srv.appspot.com/upload?name=' + encodeURIComponent(file.name) : null,
                ////resumeChunkSize: resumable ? $scope.chunkSize : null,
                //headers: {
                //    'optional-header': 'header-value'
                //},
                //data: { username: $scope.username, file: file }
                url: '/file/Upload?name=' + file.name + "&type=" + type,
                data: file
            }).then(function (response) {
                $log.debug('uploadUsingUpload: Success ' + response.config.data.file.name + 'uploaded. Response: ' + response.data);
                $timeout(function () {
                    file.result = response.data;
                });
            }, function (response) {
                $log.debug('uploadUsingUpload: Error status: ' + response.status);
                if (response.status > 0)
                    $scope.errorMsg = response.status + ': ' + response.data;
                file.error = 1;
            }, function (evt) {
                // Math.min is to fix IE which reports 200% sometimes
                file.progress = Math.min(100, parseInt(100.0 * evt.loaded / evt.total));
                $log.debug('uploadUsingUpload: progress: ' + file.progress + '% ' + evt.config.data.file.name);
            });
            /*$log.debug('uploadUsingUpload (1).');
            file.upload.xhr(function (xhr) {
                // xhr.upload.addEventListener('abort', function(){console.log('abort complete')}, false);
            });
            $log.debug('uploadUsingUpload (2).');*/
        }
    
        function uploadUsing$http(file, type) {
            $log.debug('uploadUsing$http...');
            file.upload = Upload.http({
                //url: 'https://angular-file-upload-cors-srv.appspot.com/upload' + $scope.getReqParams(),
                //method: 'POST',
                //headers: {
                //    'Content-Type': file.type
                //},
                //data: file
                url: '/file/Upload?name=' + file.name + "&type=" + type,
                data: file
            });
    
            file.upload.then(function (response) {
                file.result = response.data;
            }, function (response) {
                if (response.status > 0)
                    $scope.errorMsg = response.status + ': ' + response.data;
            });
    
            file.upload.progress(function (evt) {
                file.progress = Math.min(100, parseInt(100.0 * evt.loaded / evt.total));
            });
        }
   
        $scope.getReqParams = function () {
            $log.debug('$scope.getReqParams...');
            return $scope.generateErrorOnServer ? '?errorCode=' + $scope.serverErrorCode +
            '&errorMessage=' + $scope.serverErrorMsg : '';
        };
    
        angular.element(window).bind('dragover', function (e) {
            $log.debug('angular.element(window).bind: dragover...');
            e.preventDefault();
        });
        angular.element(window).bind('drop', function (e) {
            $log.debug('angular.element(window).drop: dragover...');
            e.preventDefault();
        });

    };

    ///////////////////////

    function PostShowActions() {
        $("#btnPostsAdd").show();
        $("#btnPostsSettings").show();
    }

})();