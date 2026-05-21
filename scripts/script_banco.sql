CREATE DATABASE LaVaiPizzaDb;
GO
USE LaVaiPizzaDb;
GO

CREATE TABLE cliente (
    id_cliente INT PRIMARY KEY IDENTITY(1,1),
    nome VARCHAR(150) NOT NULL,
    telefone VARCHAR(20),
    email VARCHAR(100),
    endereco NVARCHAR(MAX) NOT NULL
);

CREATE TABLE login (
    id_login INT PRIMARY KEY IDENTITY(1,1),
    email VARCHAR(100) UNIQUE NOT NULL,
    senha VARCHAR(255) NOT NULL,
    nome VARCHAR(100) NOT NULL
);

CREATE TABLE pizza (
    id_pizza INT PRIMARY KEY IDENTITY(1,1),
    nome VARCHAR(100) NOT NULL,
    tamanho VARCHAR(30) NOT NULL,
    preco DECIMAL(10, 2) NOT NULL
);

CREATE TABLE funcionario (
    id_funcionario INT PRIMARY KEY IDENTITY(1,1),
    nome VARCHAR(150) NOT NULL,
    cargo VARCHAR(50) NOT NULL,
    telefone VARCHAR(20),
    id_login INT UNIQUE REFERENCES login(id_login) ON DELETE SET NULL
);

CREATE TABLE pedido_entrega (
    id_pedido INT PRIMARY KEY IDENTITY(1,1),
    data_hora DATETIME DEFAULT GETDATE(),
    status VARCHAR(50) DEFAULT 'Em Preparação',
    valor_total DECIMAL(10, 2),
    endereco NVARCHAR(MAX),
    tempo_estimado INT,
    id_cliente INT NOT NULL REFERENCES cliente(id_cliente) ON DELETE CASCADE,
    id_funcionario_prepara INT REFERENCES funcionario(id_funcionario),
    id_funcionario_entrega INT REFERENCES funcionario(id_funcionario)
);

CREATE TABLE contem (
    id_pedido INT REFERENCES pedido_entrega(id_pedido) ON DELETE CASCADE,
    id_pizza INT REFERENCES pizza(id_pizza) ON DELETE CASCADE,
    quantidade INT DEFAULT 1,
    PRIMARY KEY (id_pedido, id_pizza)
);

USE LaVaiPizzaDb;
GO

-- 1. Inserir Clientes
INSERT INTO cliente (nome, telefone, email, endereco) VALUES 
('João Silva', '11988887777', 'joao@email.com', 'Rua das Flores, 123'),
('Maria Oliveira', '11977776666', 'maria@email.com', 'Av. Paulista, 1500'),
('Carlos Souza', '11966665555', 'carlos@email.com', 'Rua Amazonas, 45'),
('Ana Costa', '11955554444', 'ana@email.com', 'Rua Sergipe, 890'),
('Pedro Santos', '11944443333', 'pedro@email.com', 'Rua da Paz, 10');

-- 2. Inserir Logins (necessários para os funcionários)
INSERT INTO login (email, senha, nome) VALUES 
('admin@lavai.com', '123456', 'Administrador'),
('pizzaiolo1@lavai.com', 'pizza123', 'Roberto Massa'),
('pizzaiolo2@lavai.com', 'pizza123', 'Marcos Forno'),
('motoboy1@lavai.com', 'entrega123', 'Ricardo Veloz'),
('motoboy2@lavai.com', 'entrega123', 'Lucas Rápido');

-- 3. Inserir Pizzas
INSERT INTO pizza (nome, tamanho, preco) VALUES 
('Mussarela Especial', 'Grande', 45.90),
('Calabresa Premium', 'Grande', 49.90),
('Portuguesa LaVai', 'Média', 38.00),
('Frango com Catupiry', 'Grande', 52.00),
('Chocolate com MM', 'Broto', 29.90);

-- 4. Inserir Funcionários (ligando aos logins criados acima)
INSERT INTO funcionario (nome, cargo, telefone, id_login) VALUES 
('Roberto Massa', 'Pizzaiolo', '11911112222', 2),
('Marcos Forno', 'Pizzaiolo', '11922223333', 3),
('Ricardo Veloz', 'Entregador', '11933334444', 4),
('Lucas Rápido', 'Entregador', '11944445555', 5),
('Gerente Geral', 'Gerente', '11955556666', 1);

-- 5. Inserir Pedidos (Exemplos)
-- Nota: id_cliente 1 a 5, funcionários 1 a 5
INSERT INTO pedido_entrega (data_hora, status, valor_total, endereco, tempo_estimado, id_cliente, id_funcionario_prepara, id_funcionario_entrega) VALUES 
(GETDATE(), 'Entregue', 95.80, 'Rua das Flores, 123', 40, 1, 1, 3),
(GETDATE(), 'Em Preparação', 49.90, 'Av. Paulista, 1500', 30, 2, 2, 4),
(GETDATE(), 'Aguardando Coleta', 87.00, 'Rua Amazonas, 45', 45, 3, 1, 4),
(GETDATE(), 'Saiu para Entrega', 52.00, 'Rua Sergipe, 890', 20, 4, 2, 3),
(GETDATE(), 'Em Preparação', 29.90, 'Rua da Paz, 10', 35, 5, 1, 4);

-- 6. Inserir Itens do Pedido (Tabela contem)
-- Ligando pizzas aos pedidos acima
INSERT INTO contem (id_pedido, id_pizza, quantidade) VALUES 
(1, 1, 1), (1, 2, 1), -- Pedido 1 tem 1 Mussarela e 1 Calabresa
(2, 2, 1),           -- Pedido 2 tem 1 Calabresa
(3, 3, 1), (3, 2, 1), -- Pedido 3 tem 1 Portuguesa e 1 Calabresa
(4, 4, 1),           -- Pedido 4 tem 1 Frango
(5, 5, 1);           -- Pedido 5 tem 1 Chocolate