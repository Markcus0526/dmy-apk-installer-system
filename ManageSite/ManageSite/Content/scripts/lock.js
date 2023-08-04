var Lock = function () {

    return {
        //main function to initiate the module
        init: function () {

             $.backstretch([
		        rootUri + "Content/img/bg/1.jpg",
		        rootUri + "Content/img/bg/2.jpg",
		        rootUri + "Content/img/bg/3.jpg",
		        rootUri + "Content/img/bg/4.jpg"
		        ], {
		          fade: 1000,
		          duration: 8000
		      });
        }

    };

}();