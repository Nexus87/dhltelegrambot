module TelegramBotApi
open ApiTypes

let simpleTextMessage chatId x =
    {
        chat_id = chatId;
        text = x;
        parse_mode = None;
        disable_web_page_preview = None;
        disable_notification = None;
        reply_to_message_id = None;
    }

let getUpdate = ApiRequests.getUpdates
let sendMessage = ApiRequests.sendMessage