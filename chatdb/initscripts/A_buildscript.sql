SET search_path TO public;

create table customer (
    id serial primary key,
    name text not null
);

create table chat_message (
    id serial primary key,
    message_text text not null,
    created_at timestamp not null default now()
);