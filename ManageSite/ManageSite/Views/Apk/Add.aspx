<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<%@ Import Namespace="ManageSite.Models" %>
<%@ Import Namespace="ManageSite.Models.Library" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
	<!-- BEGIN PAGE CONTAINER-->        
	<div class="container-fluid">
		<!-- BEGIN PAGE HEADER-->
		<div class="row-fluid">
			<div class="span12">
				<!-- BEGIN PAGE TITLE & BREADCRUMB-->
				<h3 class="page-title">
					安装包管理 <small>添加/编辑安装包信息</small>
				</h3>
                
				<ul class="breadcrumb">
                    <%=Html.MvcSiteMap().SiteMapPath(" &gt; ") %>
				</ul>
				<!-- END PAGE TITLE & BREADCRUMB-->
			</div>
		</div>
		<!-- END PAGE HEADER-->
		<!-- BEGIN PAGE CONTENT-->
		<div class="row-fluid">
			<div class="span12">
				<!-- BEGIN FORM-->
                <form id="apkfileform" name="apkfileform" action="<%= ViewData["rootUri"] %>Upload/UploadFile" method="post" enctype="multipart/form-data">
                    <input type="file" id="uploadfile" name="uploadfile" onchange="showSelectedFile();" style="display:none;" />
                </form>
				<form action="<%= ViewData["rootUri"] %>apk/Add" method="post" id="form_edit" name="form_edit" class="form-horizontal">
					<div class="alert alert-error hide">
						<button class="close" data-dismiss="alert"></button>
						<span id="errormsg"></span>
					</div>
					<div class="alert alert-success hide">
						<button class="close" data-dismiss="alert"></button>
						正在上传APK文件，请稍等一会儿...
					</div>
					<div class="control-group">
						<label class="control-label" for="name">APK套餐<span class="required">*</span></label>
						<div class="controls">
                            <% string[] gids = (ViewData["groupids"] == null) ? null : ViewData["groupids"].ToString().Split(','); %>
							<select id="groupid" name="groupid" data-placeholder="请点击来选择" class="span6 chosen" multiple="multiple">
                            <% foreach (PackageInfo item in (List<PackageInfo>)ViewData["grouplist"])
                               { %>
								<option value="<%= item.uid %>" data-imgpath="<%= item.imgpath %>" 
                                <% if (gids != null && gids.Contains(item.uid.ToString())) { %>selected <% } %>><%= item.name %>
                                </option>
                            <% } %>
                            </select>
						</div>
					</div>
					<div class="control-group">
						<label class="control-label" for="name">APK名称<span class="required">*</span></label>
						<div class="controls">
							<input type="text" name="name" id="name" placeholder="请输入安装包名称" data-required="1" class="span6 m-wrap" 
                                value="<% if (ViewData["name"] != null) { %><%= ViewData["name"] %><% } %>"/>
						</div>
					</div>
					<div class="control-group">
						<label class="control-label" for="imgfile">APK默认图片<span class="required">*</span></label>
						<div class="controls">
                            <input type="hidden" id="imgpath" name="imgpath" value="<% if (ViewData["imgpath"] != null) { %><%= ViewData["imgpath"] %><% } %>" />
                            <input type=button class="btn btn-small btn-primary" id='upload_btn' value="选择图片">
                            <img src="<%= ViewData["rootUri"] %>Content/img/ajax_loader.gif" style="display:none;" id="loading_photo">

                            <div id="img1" style=" padding:5px;">
                                <% if (ViewData["imgpath"] != null && !String.IsNullOrEmpty(ViewData["imgpath"].ToString()))
                                   { %>
                                    <img src="<%= ViewData["rootUri"] %><%= ViewData["imgpath"] %>" 
                                        style="width:48px; height:48px;" onmouseover="over_img(this)" onmouseout="out_img(this)" >
                                    <img src="<%= ViewData["rootUri"] %>Content/img/image_close.png" class="close_btn" onclick="removeMe('<%= ViewData["imgpath"] %>')" 
                                        onmouseover="over_close(this)" onmouseout="out_close(this)">
                                <% } %>
                            </div>
                            <span class="help-block">只能上传一张图片<b>&nbsp;,&nbsp;&nbsp;</b>建议大小为<b>&nbsp;:&nbsp;</b>48<b>&nbsp;*&nbsp;</b>48</span>
						</div>
					</div>
					<div class="control-group">
						<label class="control-label" for="name">APK文件<span class="required">*</span></label>
						<div class="controls">
                            <input type="hidden" id="selectedapk" name="selectedapk" value="<% if (ViewData["filepath"] != null) { %><%= ViewData["filepath"] %><% } %>" />
                            <input type="hidden" id="filepath" name="filepath" value="<% if (ViewData["filepath"] != null) { %><%= ViewData["filepath"] %><% } %>" />
                            <input type=button class="btn btn-small btn-primary" id='apkfilebtn' value="选择APK文件">
                            <span class="help-block">最大文件大小：500 MB</span>
                            <div class="progress" style="display:none;">
                                <div class="bar" style="width: 0%;"></div>
                                <div class="percent">100%</div>
                            </div>
                            <div id="status">
                            <% if (ViewData["filepath"] != null && !String.IsNullOrEmpty(ViewData["filepath"].ToString()))
                                { %>
                                    <%= ViewData["filepath"]%>
                                <% } %>
                            </div>
						</div>
					</div>
					<div class="control-group">
						<label class="control-label" for="version">APK版本<span class="required">*</span></label>
						<div class="controls">
							<input type="text" name="version" id="version" placeholder="请输入安装包版本" data-required="1" class="span6 m-wrap" 
                                value="<% if (ViewData["version"] != null) { %><%= ViewData["version"] %><% } %>"/>
						</div>
					</div>

					<div class="form-actions">
						<button id="btnsubmit" type="submit" class="btn blue"><i class="icon-ok"></i> 提交</button>&nbsp;
						<a href="<%= ViewData["rootUri"] %>Apk/List" class="btn">取消</a>
					</div>
                    <input type="hidden" id="uid" name="uid" value="<%= ViewData["id"] %>">
					<input type="hidden" id="crop_x" name="x" />
					<input type="hidden" id="crop_y" name="y" />
					<input type="hidden" id="crop_w" name="w" />
					<input type="hidden" id="crop_h" name="h" />
				</form>
				<!-- END FORM-->
				
                <div id="ajax-modal" class="modal hide fade" tabindex="-1">
                </div>

			</div>
		</div>
		<!-- END PAGE CONTENT-->
	</div>
	<!-- END PAGE CONTAINER-->
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="PageStyle" runat="server">
	<link rel="stylesheet" type="text/css" href="<%= ViewData["rootUri"] %>Content/plugins/jquery-ui/jquery-ui-1.10.1.custom.min.css"/>
	<link href="<%= ViewData["rootUri"] %>Content/plugins/bootstrap-modal/css/bootstrap-modal.css" rel="stylesheet" type="text/css"/>
	<link rel="stylesheet" type="text/css" href="<%= ViewData["rootUri"] %>Content/plugins/chosen-bootstrap/chosen/chosen.css" />
	<link rel="stylesheet" type="text/css" href="<%= ViewData["rootUri"] %>Content/plugins/select2/select2_metro.css" />
	<link rel="stylesheet" type="text/css" href="<%= ViewData["rootUri"] %>Content/plugins/jquery-multi-select/css/multi-select-metro.css" />

	<link href="<%= ViewData["rootUri"] %>Content/plugins/jcrop/css/jquery.Jcrop.min.css" rel="stylesheet"/>
	<link href="<%= ViewData["rootUri"] %>Content/css/pages/image-crop.css" rel="stylesheet"/>
    <style>
        .progress { position:relative; width:400px; border: 1px solid #ddd; padding: 1px; border-radius: 3px; }
        .bar { background-color: #B4F5B4; width:0%; height:20px; border-radius: 3px; }
        .percent { position:absolute; display:inline-block; top:3px; left:48%; }
    </style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="PageScripts" runat="server">
	<script type="text/javascript" src="<%= ViewData["rootUri"] %>Content/plugins/jquery-validation/dist/jquery.validate.min.js"></script>
	<script type="text/javascript" src="<%= ViewData["rootUri"] %>Content/plugins/jquery-validation/dist/additional-methods.min.js"></script>
	<script type="text/javascript" src="<%= ViewData["rootUri"] %>Content/plugins/select2/select2.min.js"></script>
	<script type="text/javascript" src="<%= ViewData["rootUri"] %>Content/plugins/chosen-bootstrap/chosen/chosen.jquery.min.js"></script>
	<script type="text/javascript" src="<%= ViewData["rootUri"] %>Content/plugins/bootstrap-wysihtml5/wysihtml5-0.3.0.js"></script> 
	<script type="text/javascript" src="<%= ViewData["rootUri"] %>Content/plugins/bootstrap-wysihtml5/bootstrap-wysihtml5.js"></script>
	<script type="text/javascript" src="<%= ViewData["rootUri"] %>Content/plugins/bootstrap-modal/js/bootstrap-modal.js"></script>
	<script type="text/javascript" src="<%= ViewData["rootUri"] %>Content/plugins/bootstrap-modal/js/bootstrap-modalmanager.js"></script>
	<script type="text/javascript" src="<%= ViewData["rootUri"] %>Content/plugins/jcrop/js/jquery.color.js"></script>
	<script type="text/javascript" src="<%= ViewData["rootUri"] %>Content/plugins/jcrop/js/jquery.Jcrop.min.js"></script>

    <script src="<%= ViewData["rootUri"] %>Content/plugins/upload/jquery.form.js"></script>
	<script src="<%= ViewData["rootUri"] %>Content/scripts/ajaxupload.js"></script>
	<script src="<%= ViewData["rootUri"] %>Content/scripts/app.js"></script>
	<script type="text/javascript">

        var $modal = $('#ajax-modal');

	    jQuery(document).ready(function () {
	        // initiate layout and plugins
	        App.init();

	        var form1 = $('#form_edit');
	        var error1 = $('.alert-error', form1);
	        var success1 = $('.alert-success', form1);

<% if (ViewData["error"] != null) { %>
	        $("#errormsg").html("<%= ViewData["error"] %>");
	        error1.show();
	        App.scrollTo(error1, -200);
<% } %>

            /*---------- Jquery From Validation setup ---------*/
	        $.validator.messages.required = "必须要填写";
	        form1.validate({
	            errorElement: 'span', //default input error message container
	            errorClass: 'help-inline', // default input error message class
	            focusInvalid: false, // do not focus the last invalid input
	            ignore: "",
	            rules: {
                    groupid: {
                        required: true,
                    },
	                name: {
	                    required: true,
	                },
                    selectedapk: {
                        required: true
                    },
	                version: {
	                    required: true,
	                }
	            },
	            invalidHandler: function (event, validator) { //display error alert on form submit              
	                success1.hide();
	                $("#errormsg").html("您必须要填下面提示的内容。 请确认下面的输入内容。");
	                error1.show();
	                App.scrollTo(error1, -200);
	            },

	            highlight: function (element) { // hightlight error inputs
	                $(element)
                        .closest('.help-inline').removeClass('ok'); // display OK icon
	                $(element)
                        .closest('.control-group').removeClass('success').addClass('error'); // set error class to the control group
	            },

	            unhighlight: function (element) { // revert the change done by hightlight
	                $(element)
                        .closest('.control-group').removeClass('error'); // set error class to the control group
	            },

	            success: function (label) {
	                label
                        .addClass('valid').addClass('help-inline ok') // mark the current input as valid and display OK icon
                    .closest('.control-group').removeClass('error').addClass('success'); // set success class to the control group
	            },

	            submitHandler: function (form) {
	                success1.show();
	                error1.hide();
                    $("#btnsubmit").addClass("disabled");
                    $("#btnsubmit").attr("disabled", "disabled");
                    if ( $('#uploadfile').val() != "") {
                        $("form[name=apkfileform]").submit();
                    } else {
                        document.form_edit.submit();
                    }
                    //form.submit();
	            }
	        });

            /*---------- Select box ---------*/
            function format(state) {
                //if (!state.id) return state.text; // optgroup
                var originOption = state.element;
                return "<img class='flag' style='width:29px; height:29px' src='" + rootUri + $(originOption).data('imgpath') + "'/>&nbsp;&nbsp;" + state.text;
            }

//             $("#groupid").select2({
//                 allowClear: true,
//                 formatResult: format,
//                 formatSelection: format,
//                 escapeMarkup: function (m) {
//                     return m;
//                 }
//             });

            /*---------- Ajax Image Upload setup ---------*/
            new AjaxUpload('#upload_btn', {
                action: rootUri + 'Upload/UploadImage',
                onSubmit : function(file , ext){
                    $('#loading_photo').show();
                    if (! (ext && /^(JPG|PNG|JPEG|GIF)$/.test( ext.toUpperCase() ))){
                        // extensiones permitidas
                        alert('错误: 只能上传图片','');
                        $('#loading_photo').hide();
                        return false;
                    }
                },
                onComplete: function(file, response){
                    var f_name = response;
                    $('#loading_photo').hide();
                    showCropDialog(f_name);
                }
            });

            /*---------- Image Crop Dialog setup ---------*/
       	    $.fn.modalmanager.defaults.resize = true;
		    $.fn.modalmanager.defaults.spinner = '<div class="loading-spinner fade" style="width: 200px; margin-left: -100px;"><img src="' + rootUri + 'Content/img/ajax-modal-loading.gif" align="middle">&nbsp;<span style="font-weight:300; color: #eee; font-size: 18px; font-family:Open Sans;">&nbsp;Loading...</span></div>';

 

            function showCropDialog(fname)
            {
		        // create the backdrop and wait for next modal to be triggered
		        $('body').modalmanager('loading');
		 
		        setTimeout(function(){
		            $modal.load(rootUri + "Upload/RetrieveCropDialogHtml?cropfile=" + fname, '', function(){
                    cropimage();
		            $modal.modal({ backdrop: 'static', keyboard: true, width: parseInt($("#imgcrop").css("width"), 10) + 400})
                        .on("hidden", function() {
              	            $modal.empty();
                        });
		            });
		        }, 600);
            }

            $("#apkfilebtn").click(function(){
                $("input[name=uploadfile]").trigger("click");
            });
            initFormUpload();
	    });

        function over_img(obj)
        {
            clearTimeout(timeoutID);
            timer_flag = false;
            if(img_parent_div)
                $(img_parent_div).find(".close_btn").css('visibility', 'hidden');
            var obj_parent = $(obj).parent();
            //$(obj_parent).find(".close_btn").show();
            $(obj_parent).find(".close_btn").css('visibility', 'visible');
        }
        var img_parent_div = null;
        var timeoutID;
        function out_img(obj)
        {
            img_parent_div = $(obj).parent();
            timeoutID = setTimeout("timerProc( )", 500);
            timer_flag = true;
        }
        var timer_flag = false;
        var close_flag = false;
        function timerProc()
        {
            if(!close_flag)
            {
                $(img_parent_div).find(".close_btn").css('visibility', 'hidden');
            }
            timer_flag = false;
        }
        function over_close(obj)
        {
            close_flag = true;
        }
        function out_close(obj)
        {
            close_flag = false;

            if(!timer_flag)
                $(obj).css('visibility', 'hidden');
        }
        function removeMe(f_name)
        {
            var url;
            url = rootUri + "Upload/RemoveImage";
            $.ajax({
                url: url,
                data: {
                    "filename": f_name
                },
                type: "post",
                success: function(message) {
                    $('#img1').html("");
                    $('#imguri').val('');
                }
            });
        }

        var cropimage = function() {
            // Create variables (in this scope) to hold the API and image size
            //alert(parseInt($("#imgcrop").css("width"), 10) + 300);
            //$("#ajax-modal").css("width", parseInt($("#imgcrop").css("width"), 10) + 300);
            var jcrop_api,
                boundx,
                boundy,
                // Grab some information about the preview pane
                $preview = $('#preview-pane'),
                $pcnt = $('#preview-pane .preview-container'),
                $pimg = $('#preview-pane .preview-container img'),

                xsize = $pcnt.width(),
                ysize = $pcnt.height();
        
                console.log('init',[xsize,ysize]);

            $('#imgcrop').Jcrop({
              onChange: updatePreview,
              onSelect: updatePreview,
              aspectRatio: 1,
              setSelect: [ 0, 0, 170, 170]
            },function(){
              // Use the API to get the real image size
              var bounds = this.getBounds();
              boundx = bounds[0];
              boundy = bounds[1];
              // Store the API in the jcrop_api variable
              jcrop_api = this;
              // Move the preview into the jcrop container for css positioning
              $preview.appendTo(jcrop_api.ui.holder);
            });

            function updatePreview(c)
            {
                $('#crop_x').val(c.x);
                $('#crop_y').val(c.y);
                $('#crop_w').val(c.w);
                $('#crop_h').val(c.h);

                if (parseInt(c.w) > 0)
                {
                    var rx = xsize / c.w;
                    var ry = ysize / c.h;

                    $pimg.css({
                        width: Math.round(rx * boundx) + 'px',
                        height: Math.round(ry * boundy) + 'px',
                        marginLeft: '-' + Math.round(rx * c.x) + 'px',
                        marginTop: '-' + Math.round(ry * c.y) + 'px'
                    });
                }
            };
        }

        var submitCrop = function(cropfile) {
		    $modal.modal('loading');
            $.ajax({
                url: rootUri + "Upload/ResizeImage",
                data: {
                    x: $('#crop_x').val(),
                    y: $('#crop_y').val(),
                    w: $('#crop_w').val(),
                    h: $('#crop_h').val(),
                    imgpath: cropfile,
                    kind: "<%= UpImageCategory.APK %>",
                    size: "<%= CropImageSizes.APK %>"
                },
                type: "POST",
                success: function(rst) {
		            $modal.modal('loading');
                    if (rst == "") {
                        $modal.find('.modal-body')
		                    .prepend('<div class="alert alert-error fade in">' +
		                    '操作失败：原图不存在！<button type="button" class="close" data-dismiss="alert"></button>' +
		                    '</div>');
                    } else {
                        var str_html = "<img src='" + rootUri + rst+ "' style='width:48px; height:48px;' onmouseover='over_img(this)' onmouseout='out_img(this)' >";
                        str_html +=  "<img src='" + rootUri + "Content/img/image_close.png' class='close_btn' onclick='removeMe(\""+rst+"\")' onmouseover='over_close(this)' onmouseout='out_close(this)'>";
                        $('#img1').html(str_html);
                        $('#imgpath').val(rst);
                        $modal.modal('hide');
                    }
                }
            });            
        }

        var initFormUpload = function() {
            var bar = $('.bar');
            var percent = $('.percent');
            var status = $('#status');

            $('form[name=apkfileform]').ajaxForm({
                beforeSend: function () {
                    status.empty();
                    $(".progress").css("display", "block");
                    var percentVal = '0%';
                    bar.width(percentVal)
                    percent.html(percentVal);
                },
                uploadProgress: function (event, position, total, percentComplete) {
                    var percentVal = percentComplete + '%';
                    bar.width(percentVal)
                    percent.html(percentVal);
                },
                success: function () {
                    var percentVal = '100%';
                    bar.width(percentVal)
                    percent.html(percentVal);
                },
                complete: function (xhr) {
                    //status.html(xhr.responseText);
                    //$("form[name=form_edit]").submit();
                    $("#filepath").attr("value", xhr.responseText);
                    document.form_edit.submit();
                }
            });
        };

        function showSelectedFile()
        {
            var ext = $('#uploadfile').val().split('.').pop();
            if (! (ext && /^(APK)$/.test( ext.toUpperCase() ))){
                alert("请选择APK文件");
            } else {
                $('#status').html($("#uploadfile").val());
                $('#selectedapk').attr("value", $('#uploadfile').val());
            }
        }
    </script>
</asp:Content>
