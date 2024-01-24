SET search_path TO public;

create table message (
    id serial primary key,
    message_text text not null,
    username text not null,
    created_at timestamp not null default now()
);

create table picture (
    id serial primary key,
    belongs_to integer not null references message(id),
    name_of_file text not null
)