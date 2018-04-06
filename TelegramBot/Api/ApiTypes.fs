module ApiTypes

type User = { 
    id: int;
    is_bot: bool;
    first_name: string;
    last_name: string option;
    username: string option;
    language_code: string option;
}

type ChatPhoto = {
    file_id: string;
    width: int;
    height: int;
    file_size: int option;
}


type MessageEntity = { 
    ``type``: string;
    offset: int;
    length: int;
    url: string option;
    user: User option;
}
type Audio = { 
    file_id: string;
    duration: int;
    performer: string option;
    title: string option;
    mime_type: string option;
    file_size: int option;
}
type PhotoSize = { 
    file_id: string;
    width: int;
    height: int;
    file_size: int option;
}

type Document = { 
    file_id: string;
    thumb: PhotoSize option;
    file_name: string option;
    mime_type: string option;
    file_size: int option;
}

type Animation = {
    file_id: string;
    thumb: PhotoSize option;
    file_name: string option;
    mime_type: string option;
    file_size: int option;
}
type Game = { 
    title: string;
    description: string;
    photo: PhotoSize array;
    text: string option;
    text_entities:MessageEntity array option;
    animation: Animation option;
}

type MaskPosition = {
    point: string;
    x_shift: float;
    y_shift: float;
    scale: float;
}

type Sticker = { 
    file_id: string;
    width: int;
    height: int;
    thumb: PhotoSize option;
    emoji: string option;
    set_name: string option;
    mask_position: MaskPosition option;
    file_size: int option;
}
type Video = { 
    file_id: string;
    width: int;
    height: int;
    duration: int;
    thumb: PhotoSize option;
    mime_type: string option;
    file_size: int option;
}
type VideoNote = {
    file_id: string;
    length: int;
    duration: int;
    thumb: PhotoSize option;
    file_size: int option;
}
type Voice = { 
    file_id: string;
    duration: int;
    mime_type: string option;
    file_size: int option;
}
type Contact = { 
    phone_number: string;
    first_name: string;
    last_name: string option;
    user_id: int option;
}
type Location = { 
    longitude: float;
    latitude: float;
}
type Venue = { 
    location: Location;
    title: string;
    address: string;
    foursquare_id: string option;
}
type Invoice = { x: int }
type SuccessfulPayment = { x: int }

type Message = {
    message_id: int;
    from: User option;
    date: int64;
    chat: Chat;
    forward_from: User option;
    forward_from_chat: Chat option;
    forward_from_message_id: int option;
    forward_signature: string option;
    forward_date: int option;
    reply_to_message: Message option;
    edit_date: int option
    media_group_id : string option;
    author_signature: string option;
    text: string option;
    entities: MessageEntity array option;
    caption_entities: MessageEntity array option;
    audio: Audio option;
    document: Document option;
    game: Game option;
    photo: PhotoSize array option;
    sticker: Sticker option;
    video: Video option;
    voice: Voice option;
    video_note: VideoNote option;
    caption: string option;
    contact: Contact option;
    location: Location option;
    venue: Venue option;
    new_chat_members: User array option;
    left_chat_member: User option;
    new_chat_title : string option;
    new_chat_photo: PhotoSize array option;
    delete_chat_photo: bool option;
    group_chat_created: bool option;
    supergroup_chat_created: bool option;
    channel_chat_created: bool option;
    migrate_to_chat_id: int option;
    migrate_from_chat_id: int option;
    pinned_message: Message option;
    invoice: Invoice option;
    successful_payment: SuccessfulPayment option;
    connected_website: string option;
}
and Chat = { 
    id: int64;
    ``type``: string;
    title: string option;
    username : string option;
    first_name: string option;
    last_name: string option;
    all_members_are_administrators: bool option;
    photo: ChatPhoto option;
    description: string option;
    invite_link: string option;
    pinned_message: Message option;
    sticker_set_name: string option;
    can_set_sticker_set: bool option;
}
type Update = {
    update_id: int;
    message: Message option;
    edited_message: Message option;
    channel_post: Message option;
    edited_channel_post: Message option;
    inline_query: Message option;
}

type Response<'T> = {
    ok: bool;
    result: 'T
}

type SendMessage = {
    chat_id: int64;
    text: string;
    parse_mode: string option;
    disable_web_page_preview: bool option;
    disable_notification: bool option;
    reply_to_message_id: int option;
}