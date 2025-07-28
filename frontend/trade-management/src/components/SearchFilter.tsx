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

      <button 
        className="filter-toggle"
        onClick={() => setIsExpanded(!isExpanded)}
      >
        {isExpanded ? '🔼 收起过滤器' : '🔽 展开过滤器'}
      </button>

      {isExpanded && (
        <div className="filters-panel">
          <div className="filters-grid">
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