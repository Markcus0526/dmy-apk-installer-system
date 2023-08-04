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
					手机管理 <small>添加/编辑信息</small>
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
				<form action="<%= ViewData["rootUri"] %>Phone/Add" method="post" id="form_phone" class="form-horizontal">
					<div class="alert alert-error hide">
						<button class="close" data-dismiss="alert"></button>
						<span id="errormsg"></span>
					</div>
					<div class="alert alert-success hide">
						<button class="close" data-dismiss="alert"></button>
						您输入的内容都是合适的！
					</div>
					<div class="control-group">
						<label class="control-label" for="vendor">手机种类<span class="required">*</span></label>
						<div class="controls">
							<input type="text" name="vendor" id="vendor" placeholder="请输入手机种类" data-required="1" class="span6 m-wrap" 
                                value="<% if (ViewData["vendor"] != null) { %><%= ViewData["vendor"] %><% } %>"/>
							<span class="help-block">例如：三星</span>
						</div>
					</div>
					<div class="control-group">
						<label class="control-label" for="type">手机规格<span class="required">*</span></label>
						<div class="controls">
							<input type="text" name="type" id="type" placeholder="请输入规格" data-required="1" class="span6 m-wrap" 
                                value="<% if (ViewData["type"] != null) { %><%= ViewData["type"] %><% } %>"/>
							<span class="help-block">例如：Galaxy</span>
						</div>
					</div>
					<div class="control-group">
						<label class="control-label" for="sysver">系统版本<span class="required">*</span></label>
						<div class="controls">
							<input type="text" name="sysver" id="sysver" placeholder="请输入系统版本" data-required="1" class="span6 m-wrap" 
                                value="<% if (ViewData["sysver"] != null) { %><%= ViewData["sysver"] %><% } %>"/>
							<span class="help-block">例如：4.2.1</span>
						</div>
					</div>
					<div class="control-group">
						<label class="control-label" for="serial">序列号<span class="required">*</span></label>
						<div class="controls">
							<input type="text" name="serial" id="serial" placeholder="请输入序列号" data-required="1" class="span6 m-wrap" 
                                value="<% if (ViewData["serial"] != null) { %><%= ViewData["serial"] %><% } %>"/>
							<span class="help-block">例如：3290392XXXXX</span>
						</div>
					</div>

					<div class="form-actions">
						<button type="submit" class="btn blue"><i class="icon-ok"></i> 提交</button>&nbsp;
						<a href="<%= ViewData["rootUri"] %>Phone/List" class="btn">取消</a>
					</div>
                    <input type="hidden" id="phoneid" name="phoneid" value="<%= ViewData["id"] %>">
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
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="PageScripts" runat="server">
	<script type="text/javascript" src="<%= ViewData["rootUri"] %>Content/plugins/jquery-validation/dist/jquery.validate.min.js"></script>
	<script type="text/javascript" src="<%= ViewData["rootUri"] %>Content/plugins/jquery-validation/dist/additional-methods.min.js"></script>
	<script type="text/javascript" src="<%= ViewData["rootUri"] %>Content/plugins/bootstrap-wysihtml5/wysihtml5-0.3.0.js"></script> 
	<script type="text/javascript" src="<%= ViewData["rootUri"] %>Content/plugins/bootstrap-wysihtml5/bootstrap-wysihtml5.js"></script>
	<script type="text/javascript" src="<%= ViewData["rootUri"] %>Content/plugins/bootstrap-modal/js/bootstrap-modal.js"></script>
	<script type="text/javascript" src="<%= ViewData["rootUri"] %>Content/plugins/bootstrap-modal/js/bootstrap-modalmanager.js"></script>

	<script src="<%= ViewData["rootUri"] %>Content/scripts/app.js"></script>
	<script>
        var $modal = $('#ajax-modal');

	    jQuery(document).ready(function () {
	        // initiate layout and plugins
	        App.init();

	        var form1 = $('#form_phone');
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
	                type: { required: true },
	                vendor: { required: true },
	                sysver: { required: true },
	                serial: { required: true }
	            },
	            messages: {
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
	    });

    </script>
</asp:Content>
