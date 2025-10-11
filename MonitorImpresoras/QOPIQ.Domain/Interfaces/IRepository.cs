using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace QOPIQ.Domain.Interfaces
{
    /// <summary>
    /// Interfaz base para repositorios genéricos
    /// </summary>
    /// <typeparam name="TEntity">Tipo de entidad</typeparam>
    public interface IRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Obtiene una entidad por ID
        /// </summary>
        /// <param name="id">ID de la entidad</param>
        /// <returns>Entidad encontrada o null</returns>
        Task<TEntity?> GetByIdAsync(Guid id);

        /// <summary>
        /// Obtiene todas las entidades
        /// </summary>
        /// <returns>Lista de entidades</returns>
        Task<IEnumerable<TEntity>> GetAllAsync();

        /// <summary>
        /// Agrega una nueva entidad
        /// </summary>
        /// <param name="entity">Entidad a agregar</param>
        Task AddAsync(TEntity entity);

        /// <summary>
        /// Actualiza una entidad
        /// </summary>
        /// <param name="entity">Entidad a actualizar</param>
        void Update(TEntity entity);

        /// <summary>
        /// Elimina una entidad
        /// </summary>
        /// <param name="entity">Entidad a eliminar</param>
        void Delete(TEntity entity);
    }
}

