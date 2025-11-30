-- Initial database setup for AuthDemo
-- This script is executed automatically when the MySQL container starts

-- Ensure the database exists (it's already created by environment variables)
USE AuthDemoDB;

-- Set proper charset and collation
ALTER DATABASE AuthDemoDB CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Create additional user if needed
-- CREATE USER IF NOT EXISTS 'authuser'@'%' IDENTIFIED BY 'authpassword';
-- GRANT ALL PRIVILEGES ON AuthDemoDB.* TO 'authuser'@'%';

-- The tables will be created automatically by Entity Framework migrations
-- when the application starts

FLUSH PRIVILEGES;