create database QuickCartDB


create table Product
(
ProductID int identity primary key,
ProductName varchar(50),
ProductPrice numeric(10),
Vendor varchar(50),
Discount int,
ProductImage varchar(1000)
)

insert into Product values('Sony Camera',20000,'Sony India',10,'https://quickcartstorage.blob.core.windows.net/products/Point_and_shoot_cameras.jpg')
insert into Product values('Samsung TV',34000,'Samsung India',20,'https://quickcartstorage.blob.core.windows.net/products/TV.jpg')
insert into Product values('Apple watch',30000,'Apple India',12,'https://quickcartstorage.blob.core.windows.net/products/watch.jpg')
insert into Product values('TMC Protien',4000,'TMC India',15,'https://quickcartstorage.blob.core.windows.net/products/supplement.jpg')
insert into Product values('US Polo Shirt',2500,'US Polo India',17,'https://quickcartstorage.blob.core.windows.net/products/shirt.jpg')
insert into Product values('Aviator Eye glass',1000,'Aviator India',20,'https://quickcartstorage.blob.core.windows.net/products/eyewear.gif')
insert into Product values('Spyker Jeans',2000,'Spyker India',50,'https://quickcartstorage.blob.core.windows.net/products/jeans.jpg')
insert into Product values('Jumpsuit',500,'Rst India',20,'https://quickcartstorage.blob.core.windows.net/products/jumpsuits.gifs')

----------------------------------------------------------------------------------------

create table Subscribers
(
subscriberID int identity primary key,
emailID varchar(100) unique

)

--This procedure will add the subscriber, so these subscribers will get special offers!
go
create proc usp_AddSubscriber
(
@emailID varchar(100)
)
as
begin
	declare @subscriberEmail varchar(100)
	begin try
	select @subscriberEmail=emailID from Subscribers where emailID=@emailID
	if(@subscriberEmail is null)
	begin
		insert into Subscribers values (@emailID)
		return 1
	end
	else
		return 0
	end try
	begin catch
		return -1
	end catch
end


declare @res int
exec @res=usp_AddSubscriber 'user1@cloudthat.com'
select @res

Go 

select * from Subscribers

--------------------------------------------------------------------------------------------

create table Customers
(
customerID int identity(1000,1) primary key,
emailID   varchar(50) unique,
FirstName varchar(50),
LastName  varchar(50),
Pincode   numeric(6),
[password] varchar(50),
userType char(1)
)

insert into Customers values('customer1@cloudthat.com','Aplha','User',231216,'cust@1234','c')
-----------------------------------------------------------------------------------------------

create table Vendors
(
vendorID   int   identity primary key,
vendorName varchar(200),
vendorEmailID  varchar(200),
vendorPassword varchar(200),
customerType char(1) default 'v'
)

insert into vendors(vendorName,vendorEmailID,vendorPassword) values('Reebok','Rebok@quickcart.com','Kmail@1234')
insert into vendors values('Adidas','Adidas@quickcart.com','Kmail@1234')
insert into vendors values('Apple','Apple@quickcart.com','Kmail@1234')
insert into vendors values('Oneplus','Oneplus@quickcart.com','Kmail@1234')


GO

--This function will perform login validation
create function ufn_ValidateLogin
( 
@userEmailID varchar(50),
@userPassword varchar(50),
@customerType varchar(2)
)
returns int
as
begin
	if(@customerType='c')
	begin
		if exists(select 1 from Customers where emailID=@userEmailID and [password]=@userPassword)
		return 1 --Pos
	end
	else if(@customerType='v')
	begin
		if exists(select 1 from Vendors where vendorEmailID=@userEmailID and vendorPassword=@userPassword)
		return 2 --Pos
	end
	else
		return 3 --Neg
	return -1 --Super Neg
end

GO

select [dbo].ufn_ValidateLogin('customer1@cloudthat.com','cust@1234','c')

go


----------------------------------------------------------------

update Product set ProductImage = 'TV.jpg' where ProductID = 2

select * from Product

drop table orders

create table orders
(
orderid int identity(1,1) primary key,
custEmail varchar(50) references Customers(emailID),
prodID int,
prodCost int,
orderdate dateTime default getDate()
)


Select * from orders
Select * from Customers
select * from [dbo].[Vendors]
select * from [dbo].[Customers]

INSERT into Customers VALUES('pankaj.dev0ps@outlook.com', 'Pankaj', 'Choudhary', 412105, 'cust@1234', 'c')


create proc usp_AddOrder
(
    @custEmail varchar(50),
    @prodId int,
    @prodCost int,
    @orderDate datetime
)
as
begin
	begin try	
		insert into orders values (@custEmail, @prodId, @prodCost, @orderDate)
		return 1
	end try
	begin catch
		return 0
	end catch
end



select schema_name(t.schema_id) as schema_name,
       t.name as table_name,
       t.create_date,
       t.modify_date
from sys.tables t
order by schema_name,
         table_name;




select schema_name(obj.schema_id) as schema_name,
       obj.name as procedure_name,
       case type
            when 'P' then 'SQL Stored Procedure'
            when 'X' then 'Extended stored procedure'
        end as type,
        substring(par.parameters, 0, len(par.parameters)) as parameters,
        mod.definition
from sys.objects obj
join sys.sql_modules mod
     on mod.object_id = obj.object_id
cross apply (select p.name + ' ' + TYPE_NAME(p.user_type_id) + ', ' 
             from sys.parameters p
             where p.object_id = obj.object_id 
                   and p.parameter_id != 0 
             for xml path ('') ) par (parameters)
where obj.type in ('P', 'X')
order by schema_name,
         procedure_name;