using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DementiaAssistantPOC.Data
{
    public class DatabaseConnectionTests
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        
        public DatabaseConnectionTests(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        
        public async Task RunAllConnectionTestsAsync()
        {
            Console.WriteLine("\n🔌 PRUEBAS DE CONEXIÓN A BASE DE DATOS\n");
            Console.WriteLine(new string('=', 60));
            
            await Test1_CanConnect();
            await Test2_CanQuery();
            await Test3_CanInsert();
            await Test4_CanUpdate();
            await Test5_CanDelete();
            await Test6_CanQueryWithRelations();
            await Test7_CanUseTransactions();
            await Test8_PerformanceTest();
            
            Console.WriteLine("\n✅ PRUEBAS DE CONEXIÓN COMPLETADAS");
        }
        
        private async Task Test1_CanConnect()
        {
            Console.WriteLine("\n📝 Test 1: Verificar conexión básica");
            
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                Console.WriteLine($"   Conexión exitosa: {(canConnect ? "✅" : "❌")}");
                
                if (canConnect)
                {
                    var connectionString = _context.Database.GetConnectionString();
                    Console.WriteLine($"   Cadena de conexión: {connectionString}");
                    
                    var provider = _context.Database.ProviderName;
                    Console.WriteLine($"   Proveedor: {provider}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Error: {ex.Message}");
            }
        }
        
        private async Task Test2_CanQuery()
        {
            Console.WriteLine("\n📝 Test 2: Verificar consultas (SELECT)");
            
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                var people = await _context.People.ToListAsync();
                
                stopwatch.Stop();
                
                Console.WriteLine($"   Registros encontrados: {people.Count}");
                Console.WriteLine($"   Tiempo de consulta: {stopwatch.ElapsedMilliseconds}ms");
                
                foreach (var person in people.Take(3))
                {
                    Console.WriteLine($"   - {person.Name}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Error: {ex.Message}");
            }
        }
        
        private async Task Test3_CanInsert()
        {
            Console.WriteLine("\n📝 Test 3: Verificar inserción (INSERT)");
            
            try
            {
                var testPerson = new Models.Person
                {
                    Name = $"Test_{DateTime.Now:yyyyMMddHHmmss}",
                    Notes = "Persona de prueba - se eliminará"
                };
                
                _context.People.Add(testPerson);
                var result = await _context.SaveChangesAsync();
                
                Console.WriteLine($"   Filas afectadas: {result}");
                Console.WriteLine($"   ID generado: {testPerson.Id}");
                
                // Limpiar
                _context.People.Remove(testPerson);
                await _context.SaveChangesAsync();
                Console.WriteLine("   ✅ Registro de prueba eliminado");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Error: {ex.Message}");
            }
        }
        
        private async Task Test4_CanUpdate()
        {
            Console.WriteLine("\n📝 Test 4: Verificar actualización (UPDATE)");
            
            try
            {
                // Crear persona temporal
                var testPerson = new Models.Person
                {
                    Name = "Test_Update",
                    Notes = "Original"
                };
                
                _context.People.Add(testPerson);
                await _context.SaveChangesAsync();
                
                // Actualizar
                testPerson.Notes = "Actualizado";
                testPerson.Name = "Test_Updated";
                
                var result = await _context.SaveChangesAsync();
                
                Console.WriteLine($"   Filas actualizadas: {result}");
                
                // Verificar
                var updated = await _context.People.FindAsync(testPerson.Id);
                Console.WriteLine($"   Nombre actualizado: {updated?.Name}");
                Console.WriteLine($"   Notas actualizadas: {updated?.Notes}");
                
                // Limpiar
                _context.People.Remove(testPerson);
                await _context.SaveChangesAsync();
                Console.WriteLine("   ✅ Registro de prueba eliminado");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Error: {ex.Message}");
            }
        }
        
        private async Task Test5_CanDelete()
        {
            Console.WriteLine("\n📝 Test 5: Verificar eliminación (DELETE)");
            
            try
            {
                // Crear persona temporal
                var testPerson = new Models.Person
                {
                    Name = "Test_Delete",
                    Notes = "Se eliminará"
                };
                
                _context.People.Add(testPerson);
                await _context.SaveChangesAsync();
                
                var id = testPerson.Id;
                Console.WriteLine($"   Creada persona temporal con ID: {id.Substring(0, 8)}...");
                
                // Eliminar
                _context.People.Remove(testPerson);
                var result = await _context.SaveChangesAsync();
                
                Console.WriteLine($"   Filas eliminadas: {result}");
                
                // Verificar que no existe
                var deleted = await _context.People.FindAsync(id);
                Console.WriteLine($"   Persona eliminada: {deleted == null}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Error: {ex.Message}");
            }
        }
        
        private async Task Test6_CanQueryWithRelations()
        {
            Console.WriteLine("\n📝 Test 6: Verificar consultas con relaciones (JOIN)");
            
            try
            {
                var peopleWithRelations = await _context.People
                    .Include(p => p.Relationships)
                        .ThenInclude(r => r.RelatedPerson)
                    .Include(p => p.Routines)
                    .Include(p => p.LifeEvents)
                    .Take(2)
                    .ToListAsync();
                
                foreach (var person in peopleWithRelations)
                {
                    Console.WriteLine($"\n   👤 {person.Name}:");
                    Console.WriteLine($"      Relaciones: {person.Relationships.Count}");
                    Console.WriteLine($"      Rutinas: {person.Routines.Count}");
                    Console.WriteLine($"      Eventos: {person.LifeEvents.Count}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Error: {ex.Message}");
            }
        }
        
        private async Task Test7_CanUseTransactions()
        {
            Console.WriteLine("\n📝 Test 7: Verificar transacciones");
            
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    // Insertar múltiples registros
                    var person1 = new Models.Person { Name = "Test_TX_1" };
                    var person2 = new Models.Person { Name = "Test_TX_2" };
                    
                    _context.People.AddRange(person1, person2);
                    await _context.SaveChangesAsync();
                    
                    Console.WriteLine($"   Insertadas {2} personas en transacción");
                    
                    // Commit
                    await transaction.CommitAsync();
                    Console.WriteLine("   ✅ Transacción confirmada");
                    
                    // Limpiar
                    _context.People.RemoveRange(person1, person2);
                    await _context.SaveChangesAsync();
                    Console.WriteLine("   ✅ Registros de prueba eliminados");
                }
                catch
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("   ❌ Transacción revertida");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Error: {ex.Message}");
            }
        }
        
        private async Task Test8_PerformanceTest()
        {
            Console.WriteLine("\n📝 Test 8: Prueba de rendimiento");
            
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                // Consulta simple
                var count = await _context.People.CountAsync();
                
                // Consulta con filtro
                var activeRoutines = await _context.PersonRoutines
                    .Where(r => r.IsActive)
                    .CountAsync();
                
                // Consulta con relaciones
                var relationships = await _context.PersonRelationships
                    .Include(r => r.Person)
                    .Include(r => r.RelatedPerson)
                    .CountAsync();
                
                stopwatch.Stop();
                
                Console.WriteLine($"   Total personas: {count}");
                Console.WriteLine($"   Rutinas activas: {activeRoutines}");
                Console.WriteLine($"   Relaciones: {relationships}");
                Console.WriteLine($"   Tiempo total: {stopwatch.ElapsedMilliseconds}ms");
                
                if (stopwatch.ElapsedMilliseconds < 100)
                    Console.WriteLine("   ✅ Rendimiento excelente");
                else if (stopwatch.ElapsedMilliseconds < 500)
                    Console.WriteLine("   ✅ Rendimiento aceptable");
                else
                    Console.WriteLine("   ⚠️ Rendimiento podría mejorar");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Error: {ex.Message}");
            }
        }
    }
}