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
					系统管理 <small>添加/编辑信息</small>
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
				<form action="<%= ViewData["rootUri"] %>Admin/Add" method="post" id="form_admin" class="form-horizontal">
					<div class="alert alert-error hide">
						<button class="close" data-dismiss="alert"></button>
						<span id="errormsg"></span>
					</div>
					<div class="alert alert-success hide">
						<button class="close" data-dismiss="alert"></button>
						您输入的内容都是合适的！
					</div>
					<div class="control-group">
						<label class="control-label" for="name">用户账户<span class="required">*</span></label>
						<div class="controls">
							<input type="text" name="name" id="name" placeholder="请输入用户名" data-required="1" class="span6 m-wrap" 
                                value="<% if (ViewData["name"] != null) { %><%= ViewData["name"] %><% } %>"/>
							<span class="help-block">例如：wangsuxyz131, myaccount123-second</span>
						</div>
					</div>
					<div class="control-group">
						<label class="control-label" for="realname">真实姓名<span class="required">*</span></label>
						<div class="controls">
							<input type="text" name="realname" id="realname" placeholder="请输入真实姓名" data-required="1" class="span6 m-wrap"
                                value="<% if (ViewData["realname"] != null) { %><%= ViewData["realname"] %><% } %>"/>
						</div>
					</div>
					<div class="control-group">
						<label class="control-label" for="adminmailaddr">邮箱地址<span class="required">*</span></label>
						<div class="controls">
							<input type="text" name="adminmailaddr" id="adminmailaddr" placeholder="请输入邮箱地址" data-required="1" class="span6 m-wrap"
                                value="<% if (ViewData["mailaddr"] != null) { %><%= ViewData["mailaddr"] %><% } %>"/>
							<span class="help-block">例如：wangxxx.163.com</span>
						</div>
					</div>
					<div class="control-group">
						<label class="control-label" for="imgfile">用户照片<span class="required">*</span></label>
						<div class="controls">
                            <input type="hidden" id="imgpath" name="imgpath" value="<% if (ViewData["imgpath"] != null) { %><%= ViewData["imgpath"] %><% } %>" />
                            <input type=button class="btn btn-small btn-primary" id='upload_btn' value="选择图片">
                            <img src="<%= ViewData["rootUri"] %>Content/img/ajax_loader.gif" style="display:none;" id="loading_photo">

                            <div id="img1" style=" padding:5px;">
                                <% if (ViewData["imgpath"] != null && !String.IsNullOrEmpty(ViewData["imgpath"].ToString()))
                                   { %>
                                    <img src="<%= ViewData["rootUri"] %><%= ViewData["imgpath"] %>" 
                                        style="width:29px; height:29px;" onmouseover="over_img(this)" onmouseout="out_img(this)" >
                                    <img src="<%= ViewData["rootUri"] %>Content/img/image_close.png" class="close_btn" onclick="removeMe('<%= ViewData["imgpath"] %>')" 
                                        onmouseover="over_close(this)" onmouseout="out_close(this)">
                                <% } %>
                            </div>
                            <span class="help-block">&nbsp;&nbsp;&nbsp;只能上传一张图片<b>&nbsp;,&nbsp;&nbsp;</b>建议大小为<b>&nbsp;:&nbsp;</b>29<b>&nbsp;*&nbsp;</b>29</span>
						</div>
					</div>
					<div class="control-group">
						<label class="control-label" for="role">用户角色<span class="required">*</span></label>
						<div class="controls">
							<select class="span6 m-wrap" name="role" id="role">
								<option value="">请选择...</option>
                                <% foreach (Foreign_Priority item in CommonModel.GetRoleForeignList())
                                   { %>

                                   <option value="<%= item.Id %>" <% if (ViewData["role"] != null && ViewData["role"].ToString() == item.Id) { %>selected<% } %>><%= item.Name %></option>
                                <% } %>
							</select>
						</div>
					</div>
					<div class="control-group">
						<label class="control-label" for="status">状&nbsp;&nbsp;&nbsp;&nbsp;态<span class="required">*</span></label>
						<div class="controls">
                            <% foreach (Foreign_Allow item in CommonModel.GetAllowForeignList())
                               { %>
							    <label class="radio">
							    <input type="radio" name="status" value="<%= item.Id %>" <% if (ViewData["status"] != null && ViewData["status"].ToString() == item.Id.ToString() ) { %> checked<% } %> />
							    <%= item.Name %>
							    </label>
                            <% } %>
                            <div id="form_status_error"></div>
						</div>
					</div>
					<div class="control-group">
						<label class="control-label" for="password">密码<span class="required">*</span></label>
						<div class="controls">
							<input type="password" name="password" id="password" placeholder="请输入密码" data-required="1" class="span6 m-wrap"/>
						</div>
					</div>
					<div class="control-group">
						<label class="control-label" for="confirmpass">重复密码<span class="required">*</span></label>
						<div class="controls">
							<input type="password" name="confirmpass" id="confirmpass" placeholder="请输入重复密码" data-required="1" class="span6 m-wrap"/>
						</div>
					</div>
					<div class="form-actions">
						<button type="submit" class="btn blue"><i class="icon-ok"></i> 提交</button>&nbsp;
						<a href="<%= ViewData["rootUri"] %>Admin/List" class="btn">取消</a>
					</div>
                    <input type="hidden" id="adminid" name="adminid" value="<%= ViewData["id"] %>">
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
	<link href="<%= ViewData["rootUri"] %>Content/plugins/jcrop/css/jquery.Jcrop.min.css" rel="stylesheet"/>
	<link href="<%= ViewData["rootUri"] %>Content/css/pages/image-crop.css" rel="stylesheet"/>
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

	<script src="<%= ViewData["rootUri"] %>Content/scripts/ajaxupload.js"></script>
	<script src="<%= ViewData["rootUri"] %>Content/scripts/app.js"></script>
	<script>
        var $modal = $('#ajax-modal');

	    jQuery(document).ready(function () {
	        // initiate layout and plugins
	        App.init();

	        var form1 = $('#form_admin');
	        var error1 = $('.alert-error', form1);
	        var success1 = $('.alert-success', form1);

<% if (ViewData["error"] != null) { %>
	        $("#errormsg").html("<%= ViewData["error"] %>");
	        error1.show();
	        App.scrollTo(error1, -200);
<% } %>

	        $.validator.messages.required = "必须要填写";
	        //$.validator.messages.minlength = jQuery.validator.format("密码必须由至少{0}个字符组成.");
	        //$.validator.messages.maxlength = jQuery.validator.format("密码必须由最多{0}个字符组成");

	        $.validator.addMethod("loginRegex", function (value, element) {
	            return this.optional(element) || /^[a-z0-9\-]+$/i.test(value);
	        }, "用户名必须只包含字母、数字或破折号。");

	        form1.validate({
	            errorElement: 'span', //default input error message container
	            errorClass: 'help-inline', // default input error message class
	            focusInvalid: false, // do not focus the last invalid input
	            ignore: "",
	            rules: {
	                name: {
	                    required: true,
	                    loginRegex: true
	                },
	                realname: {
	                    required: true
	                },
	                adminmailaddr: {
	                    required: true,
	                    email: true
	                },
	                role: {
	                    required: true
	                },
	                status: {
	                    required: true
	                },
	                password: {
	                    required: true,
	                    minlength: 6
	                },
	                confirmpass: {
	                    required: true,
	                    equalTo: "#password",
	                    minlength: 6
	                }
	            },
	            messages: {
	                adminmailaddr: {
	                    email: "请输入正确的邮箱地址"
	                },
	                role: "请选择用户角色",
	                password: {
	                    minlength: jQuery.validator.format("密码必须由至少{0}个字符组成.")
	                },
	                confirmpass: {
	                    minlength: jQuery.validator.format("密码必须由至少{0}个字符组成."),
	                    equalTo: "请输入相同的密码"
	                }
	            },
                errorPlacement: function (error, element) { // render error placement for each input type
                    if (element.attr("name") == "status") { // for uniform radio buttons, insert the after the given container
                        error.addClass("no-left-padding").insertAfter("#form_2_membership_error");
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
                    form.submit();
	            }
	        });

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
		        }, 400);
            }
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
                    kind: "<%= UpImageCategory.ADMIN %>",
                    size: "<%= CropImageSizes.ADMIN %>"
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
                        var str_html = "<img src='" + rootUri + rst+ "' style='width:29px; height:29px;' onmouseover='over_img(this)' onmouseout='out_img(this)' >";
                        str_html +=  "<img src='" + rootUri + "Content/img/image_close.png' class='close_btn' onclick='removeMe(\""+rst+"\")' onmouseover='over_close(this)' onmouseout='out_close(this)'>";
                        $('#img1').html(str_html);
                        $('#imgpath').val(rst);
                        $modal.modal('hide');
                    }
                }
            });            
        }
    </script>
</asp:Content>
