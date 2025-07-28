# 🔍 搜索过滤功能实现指南

## 📋 功能需求

### 用户故事
- 作为用户，我希望能够快速搜索特定的贸易文档
- 作为用户，我希望能够按照不同条件过滤文档列表
- 作为用户，我希望能够组合多个过滤条件

### 功能清单
- [ ] 全局搜索框 (文档类型、公司名称、国家)
- [ ] 状态过滤器 (Draft, Pending, Approved, Rejected)
- [ ] 风险等级过滤器 (Low, Medium, High)
- [ ] 日期范围选择器
- [ ] 价值范围滑块
- [ ] 实时搜索 (输入时即时过滤)
- [ ] 清除所有过滤器按钮

---

## 🎨 前端实现

### 1. 搜索过滤组件
```tsx
// components/SearchFilter.tsx
import React, { useState, useEffect } from 'react';

interface SearchFilterProps {
  onFiltersChange: (filters: FilterOptions) => void;
  totalCount: number;
  filteredCount: number;
}

interface FilterOptions {
  searchTerm: string;
  status: string[];
  riskLevel: string[];
  dateRange: {
    from: string;
    to: string;
  };
  valueRange: {
    min: number;
    max: number;
  };
}

export const SearchFilter: React.FC<SearchFilterProps> = ({ 
  onFiltersChange, 
  totalCount, 
  filteredCount 
}) => {
  const [filters, setFilters] = useState<FilterOptions>({
    searchTerm: '',
    status: [],
    riskLevel: [],
    dateRange: { from: '', to: '' },
    valueRange: { min: 0, max: 1000000 }
  });

  const [isExpanded, setIsExpanded] = useState(false);

  useEffect(() => {
    onFiltersChange(filters);
  }, [filters, onFiltersChange]);

  const clearFilters = () => {
    setFilters({
      searchTerm: '',
      status: [],
      riskLevel: [],
      dateRange: { from: '', to: '' },
      valueRange: { min: 0, max: 1000000 }
    });
  };

  return (
    <div className="search-filter-panel">
      {/* 搜索框 */}
      <div className="search-box">
        <input
          type="text"
          placeholder="🔍 搜索文档类型、公司名称、国家..."
          value={filters.searchTerm}
          onChange={(e) => setFilters(prev => ({ 
            ...prev, 
            searchTerm: e.target.value 
          }))}
          className="search-input"
        />
        <div className="search-stats">
          显示 {filteredCount} / {totalCount} 个结果
        </div>
      </div>

      {/* 过滤器展开/收缩 */}
      <button 
        className="filter-toggle"
        onClick={() => setIsExpanded(!isExpanded)}
      >
        {isExpanded ? '🔼 收起过滤器' : '🔽 展开过滤器'}
      </button>

      {/* 过滤器面板 */}
      {isExpanded && (
        <div className="filters-panel">
          <div className="filters-grid">
            {/* 状态过滤 */}
            <div className="filter-group">
              <label>状态</label>
              <div className="checkbox-group">
                {['Draft', 'Pending', 'Approved', 'Rejected'].map(status => (
                  <label key={status} className="checkbox-label">
                    <input
                      type="checkbox"
                      checked={filters.status.includes(status)}
                      onChange={(e) => {
                        if (e.target.checked) {
                          setFilters(prev => ({
                            ...prev,
                            status: [...prev.status, status]
                          }));
                        } else {
                          setFilters(prev => ({
                            ...prev,
                            status: prev.status.filter(s => s !== status)
                          }));
                        }
                      }}
                    />
                    {status}
                  </label>
                ))}
              </div>
            </div>

            {/* 风险等级过滤 */}
            <div className="filter-group">
              <label>风险等级</label>
              <div className="checkbox-group">
                {['Low', 'Medium', 'High'].map(risk => (
                  <label key={risk} className="checkbox-label">
                    <input
                      type="checkbox"
                      checked={filters.riskLevel.includes(risk)}
                      onChange={(e) => {
                        if (e.target.checked) {
                          setFilters(prev => ({
                            ...prev,
                            riskLevel: [...prev.riskLevel, risk]
                          }));
                        } else {
                          setFilters(prev => ({
                            ...prev,
                            riskLevel: prev.riskLevel.filter(r => r !== risk)
                          }));
                        }
                      }}
                    />
                    {risk}
                  </label>
                ))}
              </div>
            </div>

            {/* 日期范围 */}
            <div className="filter-group">
              <label>创建日期</label>
              <div className="date-range">
                <input
                  type="date"
                  value={filters.dateRange.from}
                  onChange={(e) => setFilters(prev => ({
                    ...prev,
                    dateRange: { ...prev.dateRange, from: e.target.value }
                  }))}
                />
                <span>到</span>
                <input
                  type="date"
                  value={filters.dateRange.to}
                  onChange={(e) => setFilters(prev => ({
                    ...prev,
                    dateRange: { ...prev.dateRange, to: e.target.value }
                  }))}
                />
              </div>
            </div>

            {/* 价值范围 */}
            <div className="filter-group">
              <label>价值范围 (USD)</label>
              <div className="value-range">
                <input
                  type="number"
                  placeholder="最小值"
                  value={filters.valueRange.min}
                  onChange={(e) => setFilters(prev => ({
                    ...prev,
                    valueRange: { ...prev.valueRange, min: Number(e.target.value) }
                  }))}
                />
                <span>-</span>
                <input
                  type="number"
                  placeholder="最大值"
                  value={filters.valueRange.max}
                  onChange={(e) => setFilters(prev => ({
                    ...prev,
                    valueRange: { ...prev.valueRange, max: Number(e.target.value) }
                  }))}
                />
              </div>
            </div>
          </div>

          {/* 清除按钮 */}
          <div className="filter-actions">
            <button onClick={clearFilters} className="btn btn-secondary">
              🗑️ 清除所有过滤器
            </button>
          </div>
        </div>
      )}
    </div>
  );
};
```

### 2. 集成到主应用
```tsx
// App.tsx 修改
const [filteredDocuments, setFilteredDocuments] = useState<TradeDocument[]>([]);
const [filters, setFilters] = useState<FilterOptions | null>(null);

// 过滤逻辑
useEffect(() => {
  if (!filters) {
    setFilteredDocuments(documents);
    return;
  }

  let filtered = documents.filter(doc => {
    // 搜索词过滤
    if (filters.searchTerm) {
      const searchTerm = filters.searchTerm.toLowerCase();
      const matchesSearch = 
        doc.documentType.toLowerCase().includes(searchTerm) ||
        doc.companyName.toLowerCase().includes(searchTerm) ||
        doc.country.toLowerCase().includes(searchTerm);
      
      if (!matchesSearch) return false;
    }

    // 状态过滤
    if (filters.status.length > 0 && !filters.status.includes(doc.status)) {
      return false;
    }

    // 风险等级过滤
    if (filters.riskLevel.length > 0 && !filters.riskLevel.includes(doc.riskLevel)) {
      return false;
    }

    // 日期范围过滤
    if (filters.dateRange.from || filters.dateRange.to) {
      const docDate = new Date(doc.createdDate);
      if (filters.dateRange.from && docDate < new Date(filters.dateRange.from)) {
        return false;
      }
      if (filters.dateRange.to && docDate > new Date(filters.dateRange.to)) {
        return false;
      }
    }

    // 价值范围过滤
    if (doc.value < filters.valueRange.min || doc.value > filters.valueRange.max) {
      return false;
    }

    return true;
  });

  setFilteredDocuments(filtered);
}, [documents, filters]);

// 在文档管理视图中使用
{selectedView === 'documents' && (
  <section className="documents-section">
    <SearchFilter
      onFiltersChange={setFilters}
      totalCount={documents.length}
      filteredCount={filteredDocuments.length}
    />
    
    {/* 使用 filteredDocuments 而不是 documents */}
    <div className="documents-grid">
      {filteredDocuments.map(doc => (
        // 文档卡片...
      ))}
    </div>
  </section>
)}
```

---

## 🎨 CSS 样式

```css
/* 搜索过滤器样式 */
.search-filter-panel {
  background: white;
  border-radius: 12px;
  padding: 1.5rem;
  margin-bottom: 2rem;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.search-box {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 1rem;
}

.search-input {
  flex: 1;
  padding: 0.75rem 1rem;
  border: 2px solid #e1e5e9;
  border-radius: 8px;
  font-size: 1rem;
  transition: border-color 0.2s;
}

.search-input:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.search-stats {
  color: #666;
  font-size: 0.9rem;
  white-space: nowrap;
}

.filter-toggle {
  background: #f8f9fa;
  border: 1px solid #e1e5e9;
  padding: 0.5rem 1rem;
  border-radius: 6px;
  cursor: pointer;
  font-size: 0.9rem;
  transition: background-color 0.2s;
}

.filter-toggle:hover {
  background: #e9ecef;
}

.filters-panel {
  margin-top: 1rem;
  padding-top: 1rem;
  border-top: 1px solid #e1e5e9;
}

.filters-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1.5rem;
  margin-bottom: 1rem;
}

.filter-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.filter-group label {
  font-weight: 600;
  color: #333;
  font-size: 0.9rem;
}

.checkbox-group {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.checkbox-label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-weight: normal;
  cursor: pointer;
}

.date-range, .value-range {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.date-range input, .value-range input {
  flex: 1;
  padding: 0.5rem;
  border: 1px solid #e1e5e9;
  border-radius: 4px;
}

.filter-actions {
  display: flex;
  justify-content: flex-end;
  padding-top: 1rem;
  border-top: 1px solid #e1e5e9;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .filters-grid {
    grid-template-columns: 1fr;
  }
  
  .search-box {
    flex-direction: column;
    align-items: stretch;
  }
  
  .date-range, .value-range {
    flex-direction: column;
  }
}
```

---

## 🚀 实现步骤

### 1. 创建组件文件
```bash
mkdir -p frontend/trade-management/src/components
touch frontend/trade-management/src/components/SearchFilter.tsx
```

### 2. 本地测试开发
```bash
./scripts/local-test.sh
# 在 http://localhost:3000 测试功能
```

### 3. 提交代码
```bash
git add .
git commit -m "✨ Add search and filter functionality"
git push origin main
```

---

## 💡 后续增强建议

1. **后端搜索优化**: 将过滤逻辑移到后端，支持大数据量
2. **搜索历史**: 保存用户常用的搜索条件
3. **高级搜索**: 支持正则表达式和复杂查询
4. **保存过滤器**: 允许用户保存和分享过滤器配置

这个搜索过滤功能将显著提升用户体验，是一个很好的下一步开发目标！