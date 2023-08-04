<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>

<div class="modal-header">
	<button type="button" class="close" data-dismiss="modal" aria-hidden="true"></button>
	<h3>修剪图片</h3>
</div>
<div class="modal-body" style="height:310px;">
	<div>
		<div class="span6">
            <img src="<%= Model["rootUri"] %><%= Model["cropfile"] %>" style="height:<%= Model["height"] %>px; width:<%= Model["width"] %>px;" 
                id="imgcrop" name="imgcrop" />
		</div>
		<div class="span6">
			<div id="preview-pane">
				<div class="preview-container ">
					<img src="<%= Model["rootUri"] %><%= Model["cropfile"] %>" class="jcrop-preview" alt="Preview" style="height:<%= Model["height"] %>px; width:<%= Model["width"] %>px;" />
				</div>
			</div>
		</div>

	</div>
</div>
<div class="modal-footer">
	<button type="button" data-dismiss="modal" class="btn">取消</button>
	<button type="button" class="btn green" onclick="submitCrop('<%= Model["cropfile"] %>');">确定</button>
</div>