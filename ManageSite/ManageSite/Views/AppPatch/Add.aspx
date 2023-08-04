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
					Patch管理 <small>添加/编辑Patch信息</small>
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
                <form id="patchfileform" name="patchfileform" action="<%= ViewData["rootUri"] %>Upload/UploadFile" method="post" enctype="multipart/form-data">
                    <input type="file" id="uploadfile" name="uploadfile" onchange="showSelectedFile();" style="display:none;" />
                </form>
				<form action="<%= ViewData["rootUri"] %>AppPatch/Add" method="post" id="form_edit" name="form_edit" class="form-horizontal">
					<div class="alert alert-error hide">
						<button class="close" data-dismiss="alert"></button>
						<span id="errormsg"></span>
					</div>
					<div class="alert alert-success hide">
						<button class="close" data-dismiss="alert"></button>
						正在上传Patch文件，请稍等一会儿...
					</div>
					<div class="control-group">
						<label class="control-label" for="vcode">Patch版本代码<span class="required">*</span></label>
						<div class="controls">
							<input type="text" name="vcode" id="vcode" placeholder="请输入Patch版本代码" data-required="1" class="span6 m-wrap" 
                                value="<% if (ViewData["vcode"] != null) { %><%= ViewData["vcode"] %><% } %>"/>
						</div>
					</div>
					<div class="control-group">
						<label class="control-label" for="vname">Patch版本<span class="required">*</span></label>
						<div class="controls">
							<input type="text" name="vname" id="Text1" placeholder="请输入Patch版本" data-required="1" class="span6 m-wrap" 
                                value="<% if (ViewData["vname"] != null) { %><%= ViewData["vname"] %><% } %>"/>
						</div>
					</div>
					<div class="control-group">
						<label class="control-label" for="patchpath">Patch文件<span class="required">*</span></label>
						<div class="controls">
                            <input type="hidden" id="selectedpatch" name="selectedpatch" value="<% if (ViewData["patchpath"] != null) { %><%= ViewData["patchpath"] %><% } %>" />
                            <input type="hidden" id="patchpath" name="patchpath" value="<% if (ViewData["patchpath"] != null) { %><%= ViewData["patchpath"] %><% } %>" />
                            <input type=button class="btn btn-small btn-primary" id='patchfilebtn' value="选择文件">
                            <span class="help-block">最大文件大小：50 MB</span>
                            <div class="progress" style="display:none;">
                                <div class="bar" style="width: 0%;"></div>
                                <div class="percent">100%</div>
                            </div>
                            <div id="status">
                            <% if (ViewData["patchpath"] != null && !String.IsNullOrEmpty(ViewData["patchpath"].ToString()))
                                { %>
                                    <%= ViewData["patchpath"]%>
                                <% } %>
                            </div>
						</div>
					</div>
					<div class="form-actions">
						<button id="btnsubmit" type="submit" class="btn blue"><i class="icon-ok"></i> 提交</button>&nbsp;
						<a href="<%= ViewData["rootUri"] %>AppPatch/List" class="btn">取消</a>
					</div>
                    <input type="hidden" id="uid" name="uid" value="<%= ViewData["id"] %>">
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

    <style>
        .progress { position:relative; width:400px; border: 1px solid #ddd; padding: 1px; border-radius: 3px; }
        .bar { background-color: #B4F5B4; width:0%; height:20px; border-radius: 3px; }
        .percent { position:absolute; display:inline-block; top:3px; left:48%; }
    </style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="PageScripts" runat="server">
	<script type="text/javascript" src="<%= ViewData["rootUri"] %>Content/plugins/jquery-validation/dist/jquery.validate.min.js"></script>
	<script type="text/javascript" src="<%= ViewData["rootUri"] %>Content/plugins/jquery-validation/dist/additional-methods.min.js"></script>
	<script type="text/javascript" src="<%= ViewData["rootUri"] %>Content/plugins/jquery-inputmask/jquery.inputmask.bundle.min.js"></script>
	<script type="text/javascript" src="<%= ViewData["rootUri"] %>Content/plugins/select2/select2.min.js"></script>
	<script type="text/javascript" src="<%= ViewData["rootUri"] %>Content/plugins/chosen-bootstrap/chosen/chosen.jquery.min.js"></script>
	<script type="text/javascript" src="<%= ViewData["rootUri"] %>Content/plugins/bootstrap-wysihtml5/wysihtml5-0.3.0.js"></script> 
	<script type="text/javascript" src="<%= ViewData["rootUri"] %>Content/plugins/bootstrap-wysihtml5/bootstrap-wysihtml5.js"></script>
	<script type="text/javascript" src="<%= ViewData["rootUri"] %>Content/plugins/bootstrap-modal/js/bootstrap-modal.js"></script>
	<script type="text/javascript" src="<%= ViewData["rootUri"] %>Content/plugins/bootstrap-modal/js/bootstrap-modalmanager.js"></script>

    <script src="<%= ViewData["rootUri"] %>Content/plugins/upload/jquery.form.js"></script>
	<script src="<%= ViewData["rootUri"] %>Content/scripts/app.js"></script>
	<script type="text/javascript">

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
                    vcode: {
                        required: true
                    },
	                vname: {
	                    required: true
	                },
                    selectedpatch: {
                        required: true
                    },
	                sysver: {
	                    required: true
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
                        $("form[name=patchfileform]").submit();
                    } else {
                        document.form_edit.submit();
                    }
                    //form.submit();
	            }
	        });

            $("#vcode").inputmask({ "mask": "9", "repeat": 5, "greedy": false });  // ~ mask "9" or mask "99" or ... mask "9999999999"

            $("#patchfilebtn").click(function(){
                $("input[name=uploadfile]").trigger("click");
            });
            initFormUpload();
	    });

        var initFormUpload = function() {
            var bar = $('.bar');
            var percent = $('.percent');
            var status = $('#status');

            $('form[name=patchfileform]').ajaxForm({
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
                    $("#patchpath").attr("value", xhr.responseText);
                    document.form_edit.submit();
                }
            });
        };

        function showSelectedFile()
        {
            var ext = $('#uploadfile').val().split('.').pop();
            $('#status').html($("#uploadfile").val());
            $('#selectedpatch').attr("value", $('#uploadfile').val());
        }
    </script>
</asp:Content>
