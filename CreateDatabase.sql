-- =============================================
-- Скрипт создания базы данных для Библиотеки
-- =============================================

-- 1. Создать базу данных (выполнить от имени sa или с правами sysadmin)
CREATE DATABASE LibraryDB;
GO

USE LibraryDB;
GO

-- 2. Создать таблицу Books
CREATE TABLE Books
(
    Id          INT PRIMARY KEY IDENTITY(1,1),
    Title       NVARCHAR(100) NOT NULL,
    Author      NVARCHAR(100) NOT NULL,
    PublishYear INT           NOT NULL,
    Quantity    INT           NOT NULL DEFAULT 0
);
GO

-- 3. Добавить тестовые данные (необязательно)
INSERT INTO Books (Title, Author, PublishYear, Quantity) VALUES
    (N'Мастер и Маргарита',      N'Михаил Булгаков',     1967, 5),
    (N'Война и мир',             N'Лев Толстой',          1869, 3),
    (N'Преступление и наказание',N'Фёдор Достоевский',   1866, 7),
    (N'1984',                    N'Джордж Оруэлл',        1949, 4),
    (N'Гарри Поттер и философский камень', N'Дж. К. Роулинг', 1997, 10);
GO

-- Проверка
SELECT * FROM Books;
GO
