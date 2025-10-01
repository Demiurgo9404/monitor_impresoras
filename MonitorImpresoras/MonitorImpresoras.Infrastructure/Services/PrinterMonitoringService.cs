{{ ... }}
                if (printer.Status == "Online")
                {
                    // Simular niveles de tóner - COMENTADO TEMPORALMENTE
                    // printer.BlackInkLevel = random.Next(10, 100);
                    // printer.CyanInkLevel = random.Next(10, 100);
                    // printer.MagentaInkLevel = random.Next(10, 100);
                    // printer.YellowInkLevel = random.Next(10, 100);
                    
                    // Simular contador de páginas
                    printer.PageCount += random.Next(1, 10);
                }
                else
                {
                    // Si está offline, establecer niveles en 0 - COMENTADO TEMPORALMENTE
                    // printer.BlackInkLevel = 0;
                    // printer.CyanInkLevel = 0;
                    // printer.MagentaInkLevel = 0;
                    // printer.YellowInkLevel = 0;
                }
{{ ... }}
