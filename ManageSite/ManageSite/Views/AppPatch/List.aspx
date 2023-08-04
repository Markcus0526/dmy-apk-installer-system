<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
	<!-- BEGIN PAGE CONTAINER-->        
	<div class="container-fluid">
		<!-- BEGIN PAGE HEADER-->
		<div class="row-fluid">
			<div class="span12">
				<!-- BEGIN PAGE TITLE & BREADCRUMB-->
				<h3 class="page-title">
					Patch管理 <small>Patch列表</small>
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
                <form action="<%= ViewData["rootUri"] %>AppPatch/DeletePatchs" method="post">
				    <div class="alert alert-success hide">
					    <button class="close" data-dismiss="alert"></button>
					    <span id="succmsg"></span>
				    </div>
				    <!-- BEGIN EXAMPLE TABLE PORTLET-->
				    <div class="portlet box light-grey">
					    <div class="portlet-title">
						    <div class="caption"><i class="icon-user"></i>Patch列表</div>
						    <div class="actions">
							    <a href="<%= ViewData["rootUri"] %>AppPatch/Add" class="btn blue"><i class="icon-plus"></i> 新增记录</a>
							    <a href="#modalConfirmDel" class="btn yellow" data-toggle="modal"><i class="icon-trash"></i> 删除</a>
						    </div>
					    </div>
					    <div class="portlet-body">
						    <table class="table table-striped table-bordered table-hover" id="sample_1">
							    <thead>
								    <tr>
									    <th style="width:8px;"><input type="checkbox" class="group-checkable" data-set="#sample_1 .checkboxes" /></th>
									    <th>Patch版本代码</th>
									    <th>Patch版本</th>
									    <th>创造时间</th>
									    <th>操作</th>
								    </tr>
							    </thead>
							    <tbody>

							    </tbody>
						    </table>
					    </div>
				    </div>
				    <!-- END EXAMPLE TABLE PORTLET-->

                    <input type="hidden" name="del_ids" id="del_ids" value="">
				    <div id="modalConfirmDel" class="modal hide fade" tabindex="-1" role="dialog" aria-labelledby="myModalLabel3" aria-hidden="true">
					    <div class="modal-header">
						    <button type="button" class="close" data-dismiss="modal" aria-hidden="true"></button>
						    <h3 id="myModalLabel3">确定删除</h3>
					    </div>
					    <div class="modal-body">
						    <p>您选中的项目将被删除，请确认是否删除！</p>
					    </div>
					    <div class="modal-footer">
						    <button class="btn" data-dismiss="modal" aria-hidden="true">取消</button>
						    <button class="btn blue" onclick="return del_data();">确认</button>
					    </div>
				    </div>
                </form>
			</div>
		</div>
		<!-- END PAGE CONTENT-->
	</div>
	<!-- END PAGE CONTAINER-->
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="PageStyle" runat="server">
	<link rel="stylesheet" type="text/css" href="<%= ViewData["rootUri"] %>Content/plugins/select2/select2_metro.css" />
	<link rel="stylesheet" href="<%= ViewData["rootUri"] %>Content/plugins/data-tables/DT_bootstrap.css" />
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="PageScripts" runat="server">
	<script type="text/javascript" src="<%= ViewData["rootUri"] %>Content/plugins/select2/select2.min.js"></script>
	<script type="text/javascript" src="<%= ViewData["rootUri"] %>Content/plugins/data-tables/jquery.dataTables.js"></script>
	<script type="text/javascript" src="<%= ViewData["rootUri"] %>Content/plugins/data-tables/DT_bootstrap.js"></script>
	<!-- END PAGE LEVEL PLUGINS -->
	<!-- BEGIN PAGE LEVEL SCRIPTS -->
	<script src="<%= ViewData["rootUri"] %>Content/scripts/app.js"></script>
	<script>
	    jQuery(document).ready(function () {
	        App.init();
	        if (!jQuery().dataTable) {
	            return;
	        }

<% if (ViewData["success"] != null) { %>
	        $("#succmsg").html("<%= ViewData["success"] %>");
	        $(".alert-success").show();
<% } %>
	        // begin first table
	        $('#sample_1').dataTable({
	            "bServerSide": true,
	            "bProcessing": true,
	            "sAjaxSource": rootUri + "AppPatch/RetrievePatchList", 
                "oLanguage": {
	                "sUrl": rootUri + "Content/i18n/dataTables.chinese.txt"
	            },
	            "aoColumns": [
                  { "bSortable": false },
                  { "bSortable": false },
                  null,
                  null,
                  { "bSortable": false }
                ],
	            "aLengthMenu": [
                    [5, 10, 15, 20],
                    [5, 10, 15, 20] // change per page values here
                ],
	            // set the initial value
	            "iDisplayLength": 10,
	            "sDom": "<'row-fluid'<'span6'l><'span6'f>r>t<'row-fluid'<'span6'i><'span6'p>>",
	            "sPaginationType": "bootstrap",
	            "oLanguage": {
	                "sLengthMenu": "_MENU_ records per page",
	                "oPaginate": {
	                    "sPrevious": "Prev",
	                    "sNext": "Next"
	                }
	            },
	            "aoColumnDefs": [
				    {
				        aTargets: [0],    // Column number which needs to be modified
				        fnRender: function (o, v) {   // o, v contains the object and value for the column
				            return '<input type="checkbox" name="selcheckbox" class="checkboxes" value="' + o.aData[0] + '" />';
				        },
				        sClass: 'tableCell'    // Optional - class to be applied to this table cell
				    },
				    {
				        aTargets: [4],    // Column number which needs to be modified
				        fnRender: function (o, v) {   // o, v contains the object and value for the column
				            return '<a href="' + rootUri + 'AppPatch/Add/' + o.aData[4] + '">编辑</a>';
				        },
				        sClass: 'tableCell'    // Optional - class to be applied to this table cell
				    }
                ],

	            "fnDrawCallback": function (oSettings) {
	                jQuery.uniform.update("#checkbox");
	            }
	        });

	        jQuery('#sample_1 .group-checkable').change(function () {
	            var set = jQuery(this).attr("data-set");
	            var checked = jQuery(this).is(":checked");
	            jQuery(set).each(function () {
	                if (checked) {
	                    $(this).attr("checked", true);
	                } else {
	                    $(this).attr("checked", false);
	                }
	            });
	            jQuery.uniform.update(set);
	        });

	        jQuery('#sample_1_wrapper .dataTables_filter input').addClass("m-wrap medium"); // modify table search input
	        jQuery('#sample_1_wrapper .dataTables_length select').addClass("m-wrap small"); // modify table per page dropdown
	        //jQuery('#sample_1_wrapper .dataTables_length select').select2(); // initialize select2 dropdown
	    });


	    function del_data()
	    {
		    selected_id = "";
		    $(':checkbox:checked').each(function() {
			    if ($(this).attr('name') == 'selcheckbox')
				    selected_id += $(this).attr('value') + ",";
		    });
		    if(selected_id != "")
		    {
			    $("#del_ids").val(selected_id);
			    $("form").submit();
		    }
		    else
		    {
			    //
		    }
		    return false;
	    }
    </script>
</asp:Content>
