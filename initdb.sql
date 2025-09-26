create table usuario (
  id int primary key identity(1,1),
  username varchar(10) not null unique,
  passwd char(69) not null
);

create table sesion (
  token char(36) not null primary key,
  userid int not null,
  constraint fk_sesion_usuario foreign key (userid)
    references usuario(id) on delete cascade on update cascade
);
