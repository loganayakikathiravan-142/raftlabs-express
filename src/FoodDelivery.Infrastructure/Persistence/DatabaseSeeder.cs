using FoodDelivery.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FoodDelivery.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await context.MenuItems.AnyAsync(cancellationToken))
            {
                logger.LogInformation("Database already seeded with menu items. Skipping.");
                return;
            }

            logger.LogInformation("Seeding database with realistic restaurant menu items...");

            var sampleItems = new List<MenuItem>
            {
                // Pizzas
                new(
                    name: "Artisanal Margherita Pizza",
                    description: "San Marzano tomato sauce, fresh buffalo mozzarella, aromatic sweet basil, and extra virgin olive oil on hand-stretched wood-fired dough.",
                    price: 14.99m,
                    category: "Pizza",
                    imageUrl: "https://images.unsplash.com/photo-1604382355076-af4b0eb60143?auto=format&fit=crop&w=800&q=80",
                    isAvailable: true
                ),
                new(
                    name: "Truffle & Wild Mushroom Pizza",
                    description: "Roasted cremini and shiitake mushrooms, creamy fontina cheese, white truffle emulsion, and fresh thyme on crispy crust.",
                    price: 18.50m,
                    category: "Pizza",
                    imageUrl: "https://images.unsplash.com/photo-1513104890138-7c749659a591?auto=format&fit=crop&w=800&q=80",
                    isAvailable: true
                ),
                new(
                    name: "Fiery Pepperoni Diavola",
                    description: "Hand-sliced dry-cured spicy pepperoni, hot Calabrian chili honey, crushed San Marzano tomatoes, and whole milk mozzarella.",
                    price: 16.99m,
                    category: "Pizza",
                    imageUrl: "https://images.unsplash.com/photo-1628840042765-356cda07504e?auto=format&fit=crop&w=800&q=80",
                    isAvailable: true
                ),
                new(
                    name: "Quattro Formaggi Bianca",
                    description: "White base with creamy gorgonzola dolce, aged pecorino romano, buffalo mozzarella, and provolone drizzled with mountain blossom honey.",
                    price: 17.25m,
                    category: "Pizza",
                    imageUrl: "https://images.unsplash.com/photo-1573821663912-569905455b1a?auto=format&fit=crop&w=800&q=80",
                    isAvailable: true
                ),

                // Burgers
                new(
                    name: "Prime Angus Truffle Burger",
                    description: "Freshly ground 100% Black Angus chuck & brisket patty, black truffle aioli, aged English cheddar, caramelized shallots, on toasted brioche.",
                    price: 15.99m,
                    category: "Burgers",
                    imageUrl: "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?auto=format&fit=crop&w=800&q=80",
                    isAvailable: true
                ),
                new(
                    name: "Crispy Buttermilk Chicken Burger",
                    description: "24-hour buttermilk-brined chicken breast fried golden brown, zesty spicy slaw, house dill pickles, and chipotle mayo on potato bun.",
                    price: 13.50m,
                    category: "Burgers",
                    imageUrl: "https://images.unsplash.com/photo-1625813506062-0aeb1d7a094b?auto=format&fit=crop&w=800&q=80",
                    isAvailable: true
                ),
                new(
                    name: "Smoky BBQ Bacon Smash Burger",
                    description: "Double smashed seared beef patties, hickory-smoked bacon, melted sharp American cheddar, crispy onion straws, and bourbon BBQ sauce.",
                    price: 14.75m,
                    category: "Burgers",
                    imageUrl: "https://images.unsplash.com/photo-1586190848861-99aa4a171e90?auto=format&fit=crop&w=800&q=80",
                    isAvailable: true
                ),
                new(
                    name: "Plant-Based Beyond Royale",
                    description: "Juicy plant-based patty, vegan smoked gouda, crisp romaine lettuce, heirloom beefsteak tomato, sliced red onion, and house vegan remoulade.",
                    price: 14.25m,
                    category: "Burgers",
                    imageUrl: "https://images.unsplash.com/photo-1520072959219-c595dc870360?auto=format&fit=crop&w=800&q=80",
                    isAvailable: true
                ),

                // Pastas
                new(
                    name: "Slow-Braised Beef Bolognese",
                    description: "Tender beef short-rib ragu simmered for 8 hours with root vegetables, red wine, tossed with handmade egg pappardelle and Parmigiano-Reggiano.",
                    price: 16.75m,
                    category: "Pasta",
                    imageUrl: "https://images.unsplash.com/photo-1621996346565-e3d5d628169f?auto=format&fit=crop&w=800&q=80",
                    isAvailable: true
                ),
                new(
                    name: "Penne alla Vodka with Burrata",
                    description: "Al dente penne pasta in a silky San Marzano tomato & cream reduction with crushed red pepper, finished with a whole fresh Pugliese burrata ball.",
                    price: 15.50m,
                    category: "Pasta",
                    imageUrl: "https://images.unsplash.com/photo-1551183053-bf91a1d81141?auto=format&fit=crop&w=800&q=80",
                    isAvailable: true
                ),
                new(
                    name: "Classic Fettuccine Alfredo",
                    description: "Handcrafted fettuccine spun with Normandy cultured butter, heavy cream, freshly grated aged Parmigiano, and a touch of nutmeg.",
                    price: 13.99m,
                    category: "Pasta",
                    imageUrl: "https://images.unsplash.com/photo-1645112411341-6c4fd023714a?auto=format&fit=crop&w=800&q=80",
                    isAvailable: true
                ),

                // Salads & Bowls
                new(
                    name: "Mediterranean Greek Salad",
                    description: "Crisp Persian cucumbers, vine-ripened tomatoes, Kalamata olives, thinly sliced shallots, barrel-aged Greek feta block, wild oregano vinaigrette.",
                    price: 9.99m,
                    category: "Salads",
                    imageUrl: "https://images.unsplash.com/photo-1540420773420-3366772f4999?auto=format&fit=crop&w=800&q=80",
                    isAvailable: true
                ),
                new(
                    name: "Citrus Grilled Salmon Bowl",
                    description: "Pan-seared Atlantic salmon fillet, warm tri-color quinoa, Hass avocado, steamed edamame, and baby spinach with Meyer lemon ginger dressing.",
                    price: 16.99m,
                    category: "Salads",
                    imageUrl: "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?auto=format&fit=crop&w=800&q=80",
                    isAvailable: true
                ),

                // Desserts & Beverages
                new(
                    name: "Espresso Tiramisu Classico",
                    description: "Espresso-soaked Savoiardi ladyfingers layered with fluffy mascarpone zabaglione cream and dusted with Valrhona Dutch cocoa powder.",
                    price: 7.50m,
                    category: "Desserts",
                    imageUrl: "https://images.unsplash.com/photo-1571877227200-a0d98ea607e9?auto=format&fit=crop&w=800&q=80",
                    isAvailable: true
                ),
                new(
                    name: "Belgian Molten Lava Cake",
                    description: "Warm dark chocolate cake with a rich molten chocolate center, served with fresh raspberries and powdered sugar.",
                    price: 8.25m,
                    category: "Desserts",
                    imageUrl: "https://images.unsplash.com/photo-1606313564200-e75d5e30476c?auto=format&fit=crop&w=800&q=80",
                    isAvailable: true
                ),
                new(
                    name: "Fresh Squeezed Orange Juice",
                    description: "100% pure cold-pressed Valencia oranges, freshly pressed upon order. Rich in Vitamin C with natural pulp.",
                    price: 4.99m,
                    category: "Beverages",
                    imageUrl: "https://images.unsplash.com/photo-1613478223719-2ab802602423?auto=format&fit=crop&w=800&q=80",
                    isAvailable: true
                ),
                new(
                    name: "San Pellegrino Sparkling Mineral Water",
                    description: "Naturally carbonated mineral water sourced from the Italian Alps, served chilled in a 500ml glass bottle with lime wedge.",
                    price: 3.50m,
                    category: "Beverages",
                    imageUrl: "https://images.unsplash.com/photo-1559839914-ba2ac5e7b233?auto=format&fit=crop&w=800&q=80",
                    isAvailable: true
                ),

                // Unavailable item for verifying out-of-stock behavior
                new(
                    name: "Seasonal Lobster Thermidor Mac & Cheese",
                    description: "Wild Maine lobster tail chunks in a decadent three-cheese mornay sauce with garlic panko crust. (Seasonal catch - currently sold out)",
                    price: 24.99m,
                    category: "Pasta",
                    imageUrl: "https://images.unsplash.com/photo-1543339308-43e59d6b73a6?auto=format&fit=crop&w=800&q=80",
                    isAvailable: false
                )
            };

            await context.MenuItems.AddRangeAsync(sampleItems, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Successfully seeded {Count} menu items.", sampleItems.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}
