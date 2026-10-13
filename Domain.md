Oone to many relationship between category -> product.
Step 1 — Understand the problem

Suppose we have:

Product
-----------------------
Id
Name
Description
Price
StockQuantity
CategoryId
CreatedAt
UpdatedAt

and:

Category
-----------------------
Id
Name
CreatedAt

A product belongs to one category:

Electronics
   │
   ├── iPhone
   ├── MacBook
   └── AirPods

So in SQL:

Categories
     │
     │ 1 → many
     ▼
Products

We don't want:

Product.Category = "Electronics"

as a simple string.

Why?

Imagine:

Product 1 → "Electronics"
Product 2 → "Electronics"
Product 3 → "electronics"
Product 4 → "Electronic"

Now our data is inconsistent.

Instead:

Products
CategoryId = 1

and:

Categories
Id = 1
Name = Electronics

This is a relational database relationship.