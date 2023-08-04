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
					用户管理 <small>添加/编辑信息</small>
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
				<form action="<%= ViewData["rootUri"] %>User/Add" method="post" id="form_user" class="form-horizontal">
					<div class="alert alert-error hide">
						<button class="close" data-dismiss="alert"></button>
						<span id="errormsg"></span>
					</div>
					<div class="alert alert-success hide">
						<button class="close" data-dismiss="alert"></button>
						您输入的内容都是合适的！
					</div>
					<div class="control-group">
						<label class="control-label" for="name">用户角色<span class="required">*</span></label>
						<div class="controls">
							<select id="userlevel" name="userlevel" class="span6 select2" onchange="enableParent();">
                                <option value="1" <% if (ViewData["userlevel"] != null && ViewData["userlevel"].ToString() == "1") { %>selected <% } %>>1级代理</option>
                                <option value="2" <% if (ViewData["userlevel"] != null && ViewData["userlevel"].ToString() == "2") { %>selected <% } %>>2级代理</option>
                            </select>
						</div>
					</div>
					<div class="control-group" style="<% if ((ViewData["userlevel"] != null && ViewData["userlevel"].ToString() == "1") || ViewData["userlevel"] == null) { %>display:none; <% } %>">
						<label class="control-label" for="name">一级代理<span class="required">*</span></label>
						<div class="controls">
							<select id="parentid" name="parentid" class="span6 select2">
                                <option value="">请选择...</option>
                            <% foreach (UserInfo item in (List<UserInfo>)ViewData["level1user"])
                               { %>
								<option value="<%= item.uid %>" <% if (ViewData["parentid"] != null && ViewData["parentid"].ToString() == item.uid.ToString()) { %>selected <% } %>><%= item.name %></option>
                            <% } %>
                            </select>
						</div>
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
						<a href="<%= ViewData["rootUri"] %>User/List" class="btn">取消</a>
					</div>
                    <input type="hidden" id="userid" name="userid" value="<%= ViewData["id"] %>">
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

        function enableParent()
        {
            if ($("#userlevel").val() == "1") {
                $("#parentid").parent().parent().css("display", "none");
            } else {
                $("#parentid").parent().parent().css("display", "block");
            }
        }

	    jQuery(document).ready(function () {
	        // initiate layout and plugins
	        App.init();

	        var form1 = $('#form_user');
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
                    if ($("#userlevel").val() == "2" && $("#parentid").val() == "") {
                        alert("请选择一级代理");
                    } else {
	                    success1.show();
	                    error1.hide();
                        form.submit();
                    }
	            }
	        });
	    });

    </script>
</asp:Content>
