"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chat").build();

var myId = $("#myId").val();

var receiverId = null;

const conversations = {};	// dữ liệu toàn trang

connection.start().then(function () {
	Toast.info("Kết nối kênh chat thành công");
}).catch(function (err) {
	Toast.danger("Kết nối thất bại, vui lòng thử lại sau");
	return console.error(err.toString());
});

// Nhận chuỗi tin nhắn sau khi nhấn vào user hoặc khi scroll lên đầu cuộc trò chuyện
var mesgBox = $("#mesg-box");
function getConversation(_partnerId, _lastMesgId) {
	var conv = conversations[_partnerId.toString()];

	function renderConversation(data) {
		$(data).each((i, e) => {
			if (_lastMesgId == null) {
				addMesgToBottomConversation(e);
			} else {
				addMesgToTopConversation(e);
			}
		});
	}

	if (!conv) {
		conv = conversations[_partnerId.toString()] = {
			canGetMore: true,
			conversation: []
		};
	}
	if (conv.canGetMore && conv.conversation.length == 0) {
		$.getJSON("/home/getConversation",
			{
				partnerId: _partnerId,
				lastMesgId: _lastMesgId
			}
		).then(function (data) {
			if (conv.conversation == null) {
				conv.conversation = [];
			}
			data.mesgs.reverse();
			conv.conversation = data.mesgs.concat(conv.conversation);
			conv.canGetMore = data.canGetMore;
			renderConversation(conv.conversation);
		});
	} else {
		renderConversation(conv.conversation);
	}
}

function addMesgToTopConversation(mesg) {
	var divMesg = $("<div>").addClass("mesg").text(mesg.message);
	if (mesg.senderId == myId && receiverId == mesg.receiverId) {
		var me = $("<div>").addClass("me");
		me.append(divMesg);
		mesgBox.prepend(me);
	} else if (receiverId == mesg.senderId && myId == mesg.receiverId) {
		var friend = $("<div>").addClass("friend");
		friend.append(divMesg);
		mesgBox.prepend(friend);
	}
}

function addMesgToBottomConversation(mesg) {
	var divMesg = $("<div>").addClass("mesg").text(mesg.message);
	var time = $("<small>")
		.addClass("mesg-time")
		.text(new Date(mesg.sendingTime).toLocaleString());
	if (mesg.senderId == myId && receiverId == mesg.receiverId) {
		var me = $("<div>").addClass("me");
		me.append(divMesg);
		divMesg.before(time);
		mesgBox.append(me);
	} else if (receiverId == mesg.senderId && myId == mesg.receiverId) {
		var friend = $("<div>").addClass("friend");
		friend.append(divMesg);
		divMesg.after(time);
		mesgBox.append(friend);

		// Lấy tin nhắn cuối, check và đánh dấu là đã xem (nếu hợp lệ)
		var lastMesg = conversations[receiverId].conversation[conversations[receiverId].conversation.length - 1];
		if (lastMesg && lastMesg.isSeen === false) {
			connection.invoke("SeenMessage", lastMesg.id, lastMesg.senderId, lastMesg.receiverId)
				.then(() => {
					lastMesg.isSeen = true;
				});
		}
	}
	mesgBox.scrollTop(mesgBox[0].scrollHeight);
}

/*
 * Sự kiện khi nhận tin nhắn
 */
connection.on("ReceiveMessage", (mesg) => {
	var convId = (mesg.senderId == myId ? mesg.receiverId : mesg.senderId).toString();
	conversations[convId]?.conversation.push(mesg);
	// Nếu không phải user đang chat thì hiện icon thông báo
	if (receiverId != mesg.senderId) {
		var user = $("#listUser>li>.js-user-state[data-userid=" + mesg.senderId + "]");
		if (user.next().length == 0) {
			user.after($("<i>").addClass("bi bi-chat-text"));
		}
	}
	// Hàm này sẽ check lại lần nữa
	addMesgToBottomConversation(mesg);
});

/*
 * Sự kiện khi có user kết nối/ngắt kết nối
 */
connection.on("ReceiveUserStateChange", function (onlineIds) {
	var liUsers = $(".js-user-state");
	liUsers.each((i, e) => {
		var userId = Number($(e).data("userid"));
		if (userId) {
			if (onlineIds.indexOf(userId) >= 0) {
				if ($(e).hasClass("offline")) {
					$(e).removeClass("offline")
						.addClass("online");
				}
			} else {
				if ($(e).hasClass("online")) {
					$(e).removeClass("online")
						.addClass("offline");
				}
			}
		}
	});
});

/*
 * Sự kiện khi có user đăng ký mới
 */
connection.on("GetRegister", function (id, username, fullname) {
	Toast.info(`${username} vừa đăng ký tài khoản`);
	var userItem = `<li class="list-group-item d-flex justify-content-between align-items-center cursor-pointer hover-light">
						<div class="offline js-user-state" data-userid="${id}">${fullname} <small>(${username})</small></div>
					</li>`;
	$("#listUser").prepend(userItem);
});

// Mở một cuộc trò chuyện
$(document).on("click", "#listUser>li", (ev) => {
	var liEle = $(ev.currentTarget);
	var newId = liEle.find(".js-user-state").data("userid");
	var sysMesg = $("#sys-mesg");
	var preLiActive = $(ev.currentTarget).parent().find("li.active");
	// click vào user khác user đang trò chuyện
	if (receiverId != newId) {
		receiverId = newId;
		preLiActive.removeClass("active");
		liEle.addClass("active");
		mesgBox.empty();
		liEle.find("i").remove();	// xóa icon báo tin nhắn mới
		setTimeout(getConversation, 50, newId, null);
		// Tạo url
		window.history.pushState("Chat", document.title, "/chat-with-" + newId);
	}
	// Ẩn thông báo ban đầu
	if (!sysMesg.hasClass("d-none")) {
		sysMesg.addClass("d-none");
		sysMesg.prev().removeClass("d-none");
		sysMesg.next().removeClass("d-none");
	}
	// Logic xử lý cho mobile
	if (isMobile()) {
		var partner = $("#partner");
		partner.find(">span").text(liEle.text());
		partner.removeClass("d-none");
		$("#btn-back").removeClass("d-none");

		var _class = "sm-hide";
		var ele = $("#mesg-box-container");
		ele.removeClass(_class);
		$("#listUser").parent().addClass(_class);
	} else {
		$("#mesg-text").focus();
	}
});

// Nút quay lại chỉ hiển thị trên mobile
$("#btn-back").click(function (ev) {
	window.history.pushState("Chat", document.title, "/");
	var partner = $("#partner");
	partner.addClass("d-none");
	$(ev.currentTarget).addClass("d-none");
	if (isMobile()) {
		var _class = "sm-hide";
		var ele = $("#mesg-box-container");
		ele.addClass(_class);
		$("#listUser").parent().removeClass(_class);
		$("#listUser>li.active").removeClass("active");
	}
});

// Nút đóng đoạn chat chỉ hiển thị trên màn hình lớn
$("#js-chat-close").click((ev) => {
	window.history.pushState("Chat", document.title, "/");
	var sysMesg = $("#sys-mesg");
	if (sysMesg.hasClass("d-none")) {
		sysMesg.removeClass("d-none");
		sysMesg.prev().addClass("d-none");
		sysMesg.next().addClass("d-none");
		$("#listUser>li.active").removeClass("active");
		receiverId = null;
	}
});

// Bắt sự kiện nhấn enter
$("#mesg-text").keyup((ev) => {
	if (ev.shiftKey == false && ev.keyCode == 13) {
		if (isMobile()) {
			return;	// Vô hiệu hóa phím enter ở giao diện mobile
		}
		sendMessage(receiverId);
	}
});

// Load thêm tin nhắn cũ khi scroll lên đầu
var allowScrollEvent = true;
mesgBox.scroll(function (ev) {
	var conv = conversations[receiverId];
	if (allowScrollEvent) {
		if (ev.currentTarget.scrollTop < 50 && conv && conv.canGetMore) {
			if (conv.conversation[0] == null) return;
			allowScrollEvent = false;
			$.getJSON("/home/getConversation",
				{
					partnerId: receiverId,
					lastMesgId: conv.conversation[0].id
				}
			).then(function (data) {
				data.mesgs.reverse();
				conv.conversation = data.mesgs.concat(conv.conversation);
				conv.canGetMore = data.canGetMore;
				allowScrollEvent = true;
				for (var i = data.mesgs.length - 1; i >= 0; i--) {
					addMesgToTopConversation(data.mesgs[i]);
				}
			});
		}
	}
});

function sendMessage(_receiverId) {
	var txtMesg = $("#mesg-text");
	var mesgText = txtMesg.val().replace("\n","");
	if (mesgText.trim() == "") return;
	txtMesg.val('');
	connection.invoke("SendMessage", _receiverId, mesgText)
		.catch(function (err) {
			return console.error(err.toString());
		});
}

$("#btn-send").click(function (ev) {
	sendMessage(receiverId);
});

$(window).ready((ev) => {

	// Định vị đoạn chat khi nhấn f5
	var href = location.href;
	var matches = href.match(/(?<=chat-with-)\d+/);
	var id = null;
	if (matches && matches.length) {
		id = matches[0];
		var user = $("#listUser>li>.js-user-state[data-userid=" + id + "]");
		if (user.length != 0) {
			user.click();
		} else {
			window.history.replaceState("Trang chủ", document.title, "/");
		}
	}

	// Check tin nhắn mới khi vừa truy cập trang
	$.getJSON("/home/GetUnseenMessage", null,
		function (data, textStatus, jqXHR) {
			var liUsers = $("#listUser>li");
			for (let i = 0; i < data.length; i++) {
				var item = data[i];
				if (item && !item.isSeen) {
					var user = liUsers.find(">.js-user-state[data-userid=" + item.senderId + "]");
					if (user.next().length == 0) {
						user.after($("<i>").addClass("bi bi-chat-text"));
					}
				}
			}
		}
	);
});

window.onpopstate = function (ev) {
	if (isMobile()) {
		$("#btn-back").click();
	}
}

function isMobile() {
	return window.innerWidth < 576;
}