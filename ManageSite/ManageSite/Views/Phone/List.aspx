<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
	<!-- BEGIN PAGE CONTAINER-->
	<div class="container-fluid">
		<!-- BEGIN PAGE HEADER-->
		<div class="row-fluid">
			<div class="span12">
				<!-- BEGIN PAGE TITLE & BREADCRUMB-->
				<h3 class="page-title">
					手机管理 <small>手机列表</small>
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
                <form action="<%= ViewData["rootUri"] %>Phone/DeletePhones" method="post">
				    <div class="alert alert-success hide">
					    <button class="close" data-dismiss="alert"></button>
					    <span id="succmsg"></span>
				    </div>
                    <div>
                        <table id="grid-table"></table>
                        <div id="grid-pager"></div>
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
	<link rel="stylesheet" href="<%= ViewData["rootUri"] %>Content/css/ui.jqgrid.css" />
	<link rel="stylesheet" href="<%= ViewData["rootUri"] %>Content/plugins/jquery-ui/jquery-ui-1.10.1.custom.min.css" />
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="PageScripts" runat="server">
	<script type="text/javascript" src="<%= ViewData["rootUri"] %>Content/plugins/select2/select2.min.js"></script>
	<script type="text/javascript" src="<%= ViewData["rootUri"] %>Content/plugins/data-tables/jquery.dataTables.js"></script>
	<script type="text/javascript" src="<%= ViewData["rootUri"] %>Content/plugins/jqGrid/jquery.jqGrid.min.js"></script>
	<script type="text/javascript" src="<%= ViewData["rootUri"] %>Content/plugins/jqGrid/i18n/grid.locale-cn.js"></script>

	<!-- END PAGE LEVEL PLUGINS -->
	<!-- BEGIN PAGE LEVEL SCRIPTS -->
	<script src="<%= ViewData["rootUri"] %>Content/scripts/app.js"></script>
	<script>
	    jQuery(document).ready(function () {
		var grid_selector = "#grid-table";
		var pager_selector = "#grid-pager";
	
		App.init();
		
		jQuery(grid_selector).jqGrid({
			//direction: "rtl",
			url: rootUri + "Phone/GetPhoneList",
			datatype: "json",
			height: 540,
			colNames:['编号','手机类型','手机型号','系统版本', '设备ID', 'IMEI'],
			colModel:[
				{name:'uid',index:'uid', width:40, sorttype:"int", editable: false, key:true, search:false},
				{name:'type',index:'type',width:90, editable:false},
				{name:'vendor',index:'vendor',width:90, editable:false},
				{name:'sysver',index:'sysver',width:90, editable:false},
				{name:'serial',index:'serial',width:90, editable:false},
				{name:'imei',index:'imei',width:90, editable:false}
			], 
	
			viewrecords : true,
			rowNum:10,
			rowList:[10,20,30],
			pager : pager_selector,
			altRows: true,
			//toppager: true,
			
			multiselect: false,
			//multikey: "ctrlKey",
			multiboxonly: false,
	
			loadComplete : function() {
				var table = this;
				setTimeout(function(){
					styleCheckbox(table);
					
					updateActionIcons(table);
					updatePagerIcons(table);
					enableTooltips(table);
				}, 0);
			},
	
			//editurl: rootUri+"Phone/EditPhone",//nothing is saved
			caption: "手机/应用列表",
			autowidth: true,
			subGrid: true,
			subGridOptions: {
// 				"plusicon"  : "icon-caret-right blue non-absolute",
// 				"minusicon" : "icon-caret-down blue non-absolute",
// 				"openicon"  : "icon-list-ul blue non-absolute"
			},
			subGridRowExpanded: function(subgrid_id, row_id) {
				var subgrid_table_id, pager_id;
				subgrid_table_id = subgrid_id+"_t";
				pager_id = "p_"+subgrid_table_id;
				$("#"+subgrid_id).html("<table id='"+subgrid_table_id+"' class='scroll'></table><div id='"+pager_id+"' class='scroll'></div>");
				jQuery("#"+subgrid_table_id).jqGrid({
					url:rootUri + "Phone/GetPhoneAppList?telid=" + row_id,
					datatype: "json",
					colNames: ['应用名称', '应用版本','安装时间'],
					colModel: [
						{name:'appname',index:'appname', width:80, sorttype:"int", editable: false},
						{name:'appversion',index:'appversion',width:80, editable:false},
						{name:'installtime',index:'installtime',editable:false}
					],
					rowNum:10,
					pager: pager_id,
					sortname: 'appname',
                    autowidth: true,
                    edit: false,
                    add: false,
                    del: false,
					sortorder: "desc",
					height: '100%',
					multiselect: false,
					//editurl: rootUri+"goodslevel/editlevel2?level1id=" + row_id,
					loadComplete : function() {
						var table = this;
						setTimeout(function(){
							//styleCheckbox(table);
							
							//updateActionIcons(table);
							//updatePagerLevel2Icons(table);
//							enableTooltips(table);
						}, 0);
					},

				});

				jQuery("#"+subgrid_table_id).jqGrid('navGrid',"#"+pager_id,
			        { 	//navbar options
				        edit: false,
				        add: false,
				        del: false,
				        search: false,
				        refresh: false,
				        view: false
			        },
					{
						//edit record form
						//closeAfterEdit: true,
						recreateForm: true,
						beforeShowForm : function(e) {
							var form = $(e[0]);
							form.closest('.ui-jqdialog').find('.ui-jqdialog-titlebar').wrapInner('<div class="widget-header" />')
							style_edit_form(form);
						}
					},
					{
						//new record form
						closeAfterAdd: true,
						recreateForm: true,
						viewPagerButtons: false,
						beforeShowForm : function(e) {
							var form = $(e[0]);
							form.closest('.ui-jqdialog').find('.ui-jqdialog-titlebar').wrapInner('<div class="widget-header" />')
							style_edit_form(form);
						}
					},
					{
						//delete record form
						recreateForm: true,
						beforeShowForm : function(e) {
							var form = $(e[0]);
							if(form.data('styled')) return false;
							
							form.closest('.ui-jqdialog').find('.ui-jqdialog-titlebar').wrapInner('<div class="widget-header" />')
							style_delete_form(form);
							
							form.data('styled', true);
						},
						onClick : function(e) {
							alert(1);
						}
					},
					{
						//search form
						recreateForm: true,
						afterShowSearch: function(e){
							var form = $(e[0]);
							form.closest('.ui-jqdialog').find('.ui-jqdialog-title').wrap('<div class="widget-header" />')
							style_search_form(form);
						},
						afterRedraw: function(){
							style_search_filters($(this));
						}
						,
						multipleSearch: true
						/**
						multipleGroup:true,
						showQuery: true
						*/
					},
					{
						//view record form
						recreateForm: true,
						beforeShowForm: function(e){
							var form = $(e[0]);
							form.closest('.ui-jqdialog').find('.ui-jqdialog-title').wrap('<div class="widget-header" />')
						}
					}
				)

			}

	
		});
	
		//enable search/filter toolbar
		//jQuery(grid_selector).jqGrid('filterToolbar',{defaultSearch:true,stringResult:true})
	
		//switch element when editing inline
		function aceSwitch( cellvalue, options, cell ) {
			setTimeout(function(){
				$(cell) .find('input[type=checkbox]')
						.wrap('<label class="inline" />')
					.addClass('ace ace-switch ace-switch-5')
					.after('<span class="lbl"></span>');
			}, 0);
		}
		//enable datepicker
		function pickDate( cellvalue, options, cell ) {
			setTimeout(function(){
				$(cell) .find('input[type=text]')
						.datepicker({format:'yyyy-mm-dd' , autoclose:true}); 
			}, 0);
		}
	
	
		//navButtons
		jQuery(grid_selector).jqGrid('navGrid',pager_selector,
			{ 	//navbar options
				edit: false,
				//editicon : 'icon-pencil blue',
				add: false,
				//addicon : 'icon-plus-sign purple',
				del: false,
				//delicon : 'icon-trash red',
				search: true,
				//searchicon : 'icon-search orange',
				refresh: true,
				//refreshicon : 'icon-refresh green',
				view: false
				//viewicon : 'icon-zoom-in grey',
			},
			{
				//edit record form
				//closeAfterEdit: true,
				recreateForm: true,
				beforeShowForm : function(e) {
					var form = $(e[0]);
					form.closest('.ui-jqdialog').find('.ui-jqdialog-titlebar').wrapInner('<div class="widget-header" />')
					style_edit_form(form);
				}
			},
			{
				//new record form
				closeAfterAdd: true,
				recreateForm: true,
				viewPagerButtons: false,
				beforeShowForm : function(e) {
					var form = $(e[0]);
					form.closest('.ui-jqdialog').find('.ui-jqdialog-titlebar').wrapInner('<div class="widget-header" />')
					style_edit_form(form);
				}
			},
			{
				//delete record form
				recreateForm: true,
				beforeShowForm : function(e) {
					var form = $(e[0]);
					if(form.data('styled')) return false;
					
					form.closest('.ui-jqdialog').find('.ui-jqdialog-titlebar').wrapInner('<div class="widget-header" />')
					style_delete_form(form);
					
					form.data('styled', true);
				},
				onClick : function(e) {
					alert(1);
				}
			},
			{
				//search form
				recreateForm: true,
				afterShowSearch: function(e){
					var form = $(e[0]);
					form.closest('.ui-jqdialog').find('.ui-jqdialog-title').wrap('<div class="widget-header" />')
					style_search_form(form);
				},
				afterRedraw: function(){
					style_search_filters($(this));
				}
				,
				multipleSearch: true,
				/**
				multipleGroup:true,
				showQuery: true
				*/
			},
			{
				//view record form
				recreateForm: true,
				beforeShowForm: function(e){
					var form = $(e[0]);
					form.closest('.ui-jqdialog').find('.ui-jqdialog-title').wrap('<div class="widget-header" />')
				}
			}
		)
	
	
		
		function style_edit_form(form) {
			//enable datepicker on "sdate" field and switches for "stock" field
			form.find('input[name=sdate]').datepicker({format:'yyyy-mm-dd' , autoclose:true})
				.end().find('input[name=stock]')
					  .addClass('ace ace-switch ace-switch-5').wrap('<label class="inline" />').after('<span class="lbl"></span>');
	
			//update buttons classes
			var buttons = form.next().find('.EditButton .fm-button');
			buttons.addClass('btn btn-small').find('[class*="-icon"]').remove();//ui-icon, s-icon
			buttons.eq(0).addClass('btn-primary').prepend('<i class="icon-ok"></i>');
			buttons.eq(1).prepend('<i class="icon-remove"></i>')
			
			buttons = form.next().find('.navButton a');
			buttons.find('.ui-icon').remove();
			buttons.eq(0).append('<i class="icon-chevron-left"></i>');
			buttons.eq(1).append('<i class="icon-chevron-right"></i>');		
		}
	
		function style_delete_form(form) {
			var buttons = form.next().find('.EditButton .fm-button');
			buttons.addClass('btn btn-small').find('[class*="-icon"]').remove();//ui-icon, s-icon
			buttons.eq(0).addClass('btn-danger').prepend('<i class="icon-trash"></i>');
			buttons.eq(1).prepend('<i class="icon-remove"></i>')
		}
		
		function style_search_filters(form) {
			form.find('.delete-rule').val('X');
			form.find('.add-rule').addClass('btn btn-small btn-primary');
			form.find('.add-group').addClass('btn btn-small btn-success');
			form.find('.delete-group').addClass('btn btn-small btn-danger');
		}
		function style_search_form(form) {
			var dialog = form.closest('.ui-jqdialog');
			var buttons = dialog.find('.EditTable')
			buttons.find('.EditButton a[id*="_reset"]').addClass('btn btn-small btn-info').find('.ui-icon').attr('class', 'icon-retweet');
			buttons.find('.EditButton a[id*="_query"]').addClass('btn btn-small btn-inverse').find('.ui-icon').attr('class', 'icon-comment-alt');
			buttons.find('.EditButton a[id*="_search"]').addClass('btn btn-small btn-purple').find('.ui-icon').attr('class', 'icon-search');
		}
		
		function beforeDeleteCallback(e) {

			var form = $(e[0]);
			if(form.data('styled')) return false;
			
			form.closest('.ui-jqdialog').find('.ui-jqdialog-titlebar').wrapInner('<div class="widget-header" />')
			style_delete_form(form);
			
			form.data('styled', true);
		}
		
		function beforeEditCallback(e) {
			var form = $(e[0]);
			form.closest('.ui-jqdialog').find('.ui-jqdialog-titlebar').wrapInner('<div class="widget-header" />')
			style_edit_form(form);
		}
	
	
	
		//it causes some flicker when reloading or navigating grid
		//it may be possible to have some custom formatter to do this as the grid is being created to prevent this
		//or go back to default browser checkbox styles for the grid
		function styleCheckbox(table) {
		/**
			$(table).find('input:checkbox').addClass('ace')
			.wrap('<label />')
			.after('<span class="lbl align-top" />')
	
	
			$('.ui-jqgrid-labels th[id*="_cb"]:first-child')
			.find('input.cbox[type=checkbox]').addClass('ace')
			.wrap('<label />').after('<span class="lbl align-top" />');
		*/
		}
		
	
		//unlike navButtons icons, action icons in rows seem to be hard-coded
		//you can change them like this in here if you want
		function updateActionIcons(table) {
			/**
			var replacement = 
			{
				'ui-icon-pencil' : 'icon-pencil blue',
				'ui-icon-trash' : 'icon-trash red',
				'ui-icon-disk' : 'icon-ok green',
				'ui-icon-cancel' : 'icon-remove red'
			};
			$(table).find('.ui-pg-div span.ui-icon').each(function(){
				var icon = $(this);
				var $class = $.trim(icon.attr('class').replace('ui-icon', ''));
				if($class in replacement) icon.attr('class', 'ui-icon '+replacement[$class]);
			})
			*/
		}
		
		//replace icons with FontAwesome icons like above
		function updatePagerIcons(table) {
			var replacement = 
			{
// 				'ui-icon-seek-first' : 'icon-double-angle-left bigger-140',
// 				'ui-icon-seek-prev' : 'icon-angle-left bigger-140',
// 				'ui-icon-seek-next' : 'icon-angle-right bigger-140',
// 				'ui-icon-seek-end' : 'icon-double-angle-right bigger-140'
			};
			$('.ui-pg-table:not(.navtable) > tbody > tr > .ui-pg-button > .ui-icon').each(function(){
				var icon = $(this);
				var $class = $.trim(icon.attr('class').replace('ui-icon', ''));
				
				if($class in replacement) icon.attr('class', 'ui-icon '+replacement[$class]);
			})
		}
			
		function updatePagerLevel2Icons(table) {
			var replacement = 
			{
				'ui-icon-seek-first' : 'icon-double-angle-left bigger-140',
				'ui-icon-seek-prev' : 'icon-angle-left bigger-140',
				'ui-icon-seek-next' : 'icon-angle-right bigger-140',
				'ui-icon-seek-end' : 'icon-double-angle-right bigger-140'
			};
			table.find(subgrid_table_id+ '_center .ui-icon').each(function(){
				var icon = $(this);
				var $class = $.trim(icon.attr('class').replace('ui-icon', ''));
				
				if($class in replacement) icon.attr('class', 'ui-icon '+replacement[$class]);
			})
		}
	
		function enableTooltips(table) {
			$('.navtable .ui-pg-button').tooltip({container:'body'});
			$(table).find('.ui-pg-div').tooltip({container:'body'});
		}

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
