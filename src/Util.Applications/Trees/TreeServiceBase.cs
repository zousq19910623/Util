﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Util.Applications.Dtos;
using Util.Datas.Queries.Trees;
using Util.Datas.Stores;
using Util.Datas.UnitOfWorks;
using Util.Domains;
using Util.Domains.Trees;

namespace Util.Applications.Trees {
    /// <summary>
    /// 树型服务
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <typeparam name="TDto">数据传输对象类型</typeparam>
    /// <typeparam name="TQueryParameter">查询参数类型</typeparam>
    public abstract class TreeServiceBase<TEntity, TDto, TQueryParameter>
        : TreeServiceBase<TEntity, TDto, TQueryParameter, Guid, Guid?>, ITreeService<TDto, TQueryParameter>
        where TEntity : class, IParentId<Guid?>, IPath, IEnabled, ISortId, IKey<Guid>, IVersion, new()
        where TDto : class, IDto, ITreeNode, new()
        where TQueryParameter : class, ITreeQueryParameter {
        /// <summary>
        /// 初始化树型服务
        /// </summary>
        /// <param name="unitOfWork">工作单元</param>
        /// <param name="store">存储器</param>
        protected TreeServiceBase( IUnitOfWork unitOfWork, IStore<TEntity, Guid> store ) : base( unitOfWork, store ) {
        }

        /// <summary>
        /// 过滤
        /// </summary>
        protected override IQueryable<TEntity> Filter( IQueryable<TEntity> queryable, TQueryParameter parameter ) {
            return queryable.Where( new TreeCriteria<TEntity>( parameter ) );
        }
    }

    /// <summary>
    /// 树型服务
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <typeparam name="TDto">数据传输对象类型</typeparam>
    /// <typeparam name="TQueryParameter">查询参数类型</typeparam>
    /// <typeparam name="TKey">实体标识类型</typeparam>
    /// <typeparam name="TParentId">父标识类型</typeparam>
    public abstract class TreeServiceBase<TEntity, TDto, TQueryParameter, TKey, TParentId>
        : DeleteServiceBase<TEntity, TDto, TQueryParameter, TKey>, ITreeService<TDto, TQueryParameter, TParentId>
        where TEntity : class, IParentId<TParentId>, IPath, IEnabled, ISortId, IKey<TKey>, IVersion, new()
        where TDto : class, IDto, ITreeNode, new()
        where TQueryParameter : class, ITreeQueryParameter<TParentId> {
        /// <summary>
        /// 工作单元
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;
        /// <summary>
        /// 存储
        /// </summary>
        private readonly IStore<TEntity, TKey> _store;

        /// <summary>
        /// 初始化树型服务
        /// </summary>
        /// <param name="unitOfWork">工作单元</param>
        /// <param name="store">存储器</param>
        protected TreeServiceBase( IUnitOfWork unitOfWork, IStore<TEntity, TKey> store ) : base( unitOfWork, store ) {
            _unitOfWork = unitOfWork;
            _store = store;
        }

        /// <summary>
        /// 过滤
        /// </summary>
        protected override IQueryable<TEntity> Filter( IQueryable<TEntity> queryable, TQueryParameter parameter ) {
            return queryable.Where( new TreeCriteria<TEntity, TParentId>( parameter ) );
        }

        /// <summary>
        /// 交换排序
        /// </summary>
        /// <param name="id">标识</param>
        /// <param name="swapId">目标标识</param>
        public async Task SwapSortAsync( Guid id, Guid swapId ) {
            var entity = await _store.FindAsync( id );
            var swapEntity = await _store.FindAsync( swapId );
            if( entity == null || swapEntity == null )
                return;
            entity.SwapSort( swapEntity );
            await _store.UpdateAsync( entity );
            await _store.UpdateAsync( swapEntity );
            await _unitOfWork.CommitAsync();
        }
    }
}
