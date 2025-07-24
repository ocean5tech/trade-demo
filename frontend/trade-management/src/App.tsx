import React, { useState, useEffect } from 'react';
import './App.css';

// 类型定义
interface TradeDocument {
  id: number;
  documentType: string;
  country: string;
  status: 'Draft' | 'Pending' | 'Approved' | 'Rejected';
  createdDate: string;
  companyName: string;
  value: number;
  riskLevel: 'Low' | 'Medium' | 'High';
}

interface ComplianceAlert {
  id: number;
  type: 'warning' | 'error' | 'info';
  message: string;
  documentId?: number;
}

// 动态配置 API URL - 生产环境使用相对路径通过 nginx 代理
const API_BASE_URL = process.env.NODE_ENV === 'production' 
  ? '/api'  // 生产环境使用 nginx 代理
  : 'http://localhost:5000/api';  // 本地开发

const App: React.FC = () => {
  const [documents, setDocuments] = useState<TradeDocument[]>([]);
  const [alerts, setAlerts] = useState<ComplianceAlert[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string>('');
  const [selectedView, setSelectedView] = useState<'dashboard' | 'documents' | 'compliance'>('dashboard');
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [editingDocument, setEditingDocument] = useState<TradeDocument | null>(null);
  const [formData, setFormData] = useState<Partial<TradeDocument>>({
    documentType: '',
    country: '',
    status: 'Draft',
    companyName: '',
    value: 0,
    riskLevel: 'Low'
  });

  // 数据获取逻辑
  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        setError('');

        console.log('🔄 正在调用API...', API_BASE_URL);
        
        const [documentsResponse, alertsResponse] = await Promise.all([
          fetch(`${API_BASE_URL}/tradedocuments`),
          fetch(`${API_BASE_URL}/compliancealerts`)
        ]);

        if (!documentsResponse.ok || !alertsResponse.ok) {
          throw new Error(`API请求失败: ${documentsResponse.status}`);
        }

        const documentsData = await documentsResponse.json();
        const alertsData = await alertsResponse.json();

        console.log('✅ API数据获取成功:', { documents: documentsData.length, alerts: alertsData.length });
        
        setDocuments(documentsData);
        setAlerts(alertsData);
        
      } catch (error) {
        console.error('❌ API调用失败:', error);
        setError(error instanceof Error ? error.message : '获取数据失败');
        
        // 使用备用模拟数据
        setDocuments([
          {
            id: 1,
            documentType: 'Export License',
            country: 'China',
            status: 'Approved',
            createdDate: '2025-01-15',
            companyName: 'Global Electronics Ltd',
            value: 125000,
            riskLevel: 'Low'
          },
          {
            id: 2,
            documentType: 'Import Permit',
            country: 'Germany',
            status: 'Pending',
            createdDate: '2025-01-10',
            companyName: 'Euro Manufacturing GmbH',
            value: 89000,
            riskLevel: 'Medium'
          },
          {
            id: 3,
            documentType: 'Certificate of Origin',
            country: 'USA',
            status: 'Draft',
            createdDate: '2025-01-08',
            companyName: 'American Trade Corp',
            value: 156000,
            riskLevel: 'High'
          }
        ]);
        
        setAlerts([
          {
            id: 1,
            type: 'error',
            message: '无法连接到API服务器，使用备用数据'
          },
          {
            id: 2,
            type: 'warning',
            message: '检测到高风险交易，需要额外审查'
          }
        ]);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []);

  // CRUD 操作函数
  const createDocument = async (newDocument: Partial<TradeDocument>) => {
    try {
      const response = await fetch(`${API_BASE_URL}/tradedocuments`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(newDocument),
      });

      if (!response.ok) {
        throw new Error(`创建失败: ${response.status}`);
      }

      const createdDocument = await response.json();
      setDocuments(prev => [createdDocument, ...prev]);
      setShowCreateForm(false);
      resetForm();
      console.log('✅ 文档创建成功:', createdDocument);
    } catch (error) {
      console.error('❌ 创建文档失败:', error);
      setError(error instanceof Error ? error.message : '创建文档失败');
    }
  };

  const updateDocument = async (id: number, updatedDocument: Partial<TradeDocument>) => {
    try {
      const response = await fetch(`${API_BASE_URL}/tradedocuments/${id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ ...updatedDocument, id }),
      });

      if (!response.ok) {
        throw new Error(`更新失败: ${response.status}`);
      }

      setDocuments(prev => prev.map(doc => 
        doc.id === id ? { ...doc, ...updatedDocument } : doc
      ));
      setEditingDocument(null);
      resetForm();
      console.log('✅ 文档更新成功');
    } catch (error) {
      console.error('❌ 更新文档失败:', error);
      setError(error instanceof Error ? error.message : '更新文档失败');
    }
  };

  const deleteDocument = async (id: number) => {
    if (!window.confirm('确定要删除这个文档吗？此操作不可撤销。')) {
      return;
    }

    try {
      const response = await fetch(`${API_BASE_URL}/tradedocuments/${id}`, {
        method: 'DELETE',
      });

      if (!response.ok) {
        throw new Error(`删除失败: ${response.status}`);
      }

      setDocuments(prev => prev.filter(doc => doc.id !== id));
      console.log('✅ 文档删除成功');
    } catch (error) {
      console.error('❌ 删除文档失败:', error);
      setError(error instanceof Error ? error.message : '删除文档失败');
    }
  };

  // 表单处理函数
  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: name === 'value' ? parseFloat(value) || 0 : value
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (editingDocument) {
      await updateDocument(editingDocument.id, formData);
    } else {
      await createDocument(formData);
    }
  };

  const startEdit = (document: TradeDocument) => {
    setEditingDocument(document);
    setFormData({
      documentType: document.documentType,
      country: document.country,
      status: document.status,
      companyName: document.companyName,
      value: document.value,
      riskLevel: document.riskLevel
    });
    setShowCreateForm(true);
  };

  const resetForm = () => {
    setFormData({
      documentType: '',
      country: '',
      status: 'Draft',
      companyName: '',
      value: 0,
      riskLevel: 'Low'
    });
    setEditingDocument(null);
  };

  const cancelForm = () => {
    setShowCreateForm(false);
    resetForm();
  };

  // 统计计算
  const stats = {
    totalDocuments: documents.length,
    pendingApproval: documents.filter(d => d.status === 'Pending').length,
    approvedToday: documents.filter(d => d.status === 'Approved').length,
    totalValue: documents.reduce((sum, doc) => sum + doc.value, 0),
    highRiskCount: documents.filter(d => d.riskLevel === 'High').length
  };

  // 辅助函数
  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  };

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'Approved': return '✅';
      case 'Pending': return '⏳';
      case 'Draft': return '📝';
      case 'Rejected': return '❌';
      default: return '📄';
    }
  };

  const getRiskColor = (level: string) => {
    switch (level) {
      case 'High': return '#ff4757';
      case 'Medium': return '#ffa502';
      case 'Low': return '#2ed573';
      default: return '#57606f';
    }
  };

  // 加载状态
  if (loading) {
    return (
      <div className="loading-container">
        <div className="loading-spinner"></div>
        <p>🔄 连接Trade Management API...</p>
        <p className="api-url">{API_BASE_URL}</p>
      </div>
    );
  }

  // 主渲染
  return (
    <div className="app">
      <header className="app-header">
        <div className="header-content">
          <h1>🌐 Global Trade Management</h1>
          <p>下一代贸易合规管理平台</p>
          
          {error && (
            <div className="api-warning">
              ⚠️ API状态: {error}
            </div>
          )}
          
          <div className="api-status">
            <span className={`api-indicator ${error ? 'offline' : 'online'}`}>
              {error ? '🔴' : '🟢'}
            </span>
            API: {API_BASE_URL} {error ? '(离线)' : '(在线)'}
          </div>
          
          <nav className="main-nav">
            <button 
              className={selectedView === 'dashboard' ? 'nav-btn active' : 'nav-btn'}
              onClick={() => setSelectedView('dashboard')}
            >
              📊 仪表板
            </button>
            <button 
              className={selectedView === 'documents' ? 'nav-btn active' : 'nav-btn'}
              onClick={() => setSelectedView('documents')}
            >
              📄 文档管理
            </button>
            <button 
              className={selectedView === 'compliance' ? 'nav-btn active' : 'nav-btn'}
              onClick={() => setSelectedView('compliance')}
            >
              🛡️ 合规监控
            </button>
          </nav>
        </div>
      </header>

      <main className="main-content">
        {selectedView === 'dashboard' && (
          <>
            <section className="stats-section">
              <h2>📊 执行概况</h2>
              <div className="stats-grid">
                <div className="stat-card total">
                  <h3>总文档数</h3>
                  <div className="stat-number">{stats.totalDocuments}</div>
                  <div className="stat-label">系统中活跃</div>
                </div>
                <div className="stat-card pending">
                  <h3>待审批</h3>
                  <div className="stat-number">{stats.pendingApproval}</div>
                  <div className="stat-label">等待审查</div>
                </div>
                <div className="stat-card approved">
                  <h3>已批准</h3>
                  <div className="stat-number">{stats.approvedToday}</div>
                  <div className="stat-label">可以处理</div>
                </div>
                <div className="stat-card value">
                  <h3>贸易总价值</h3>
                  <div className="stat-number">{formatCurrency(stats.totalValue)}</div>
                  <div className="stat-label">当前投资组合</div>
                </div>
              </div>
            </section>

            <section className="alerts-section">
              <h2>🚨 合规警报</h2>
              <div className="alerts-container">
                {alerts.map(alert => (
                  <div key={alert.id} className={`alert alert-${alert.type}`}>
                    <div className="alert-icon">
                      {alert.type === 'warning' && '⚠️'}
                      {alert.type === 'error' && '🚫'}
                      {alert.type === 'info' && 'ℹ️'}
                    </div>
                    <div className="alert-content">
                      <span>{alert.message}</span>
                      {alert.documentId && (
                        <button className="alert-action">查看文档</button>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            </section>
          </>
        )}

        {selectedView === 'documents' && (
          <section className="documents-section">
            <div className="section-header">
              <h2>📄 贸易文档管理</h2>
              <button 
                className="btn btn-primary"
                onClick={() => setShowCreateForm(true)}
              >
                ➕ 新建文档
              </button>
            </div>

            {showCreateForm && (
              <div className="modal-overlay">
                <div className="modal">
                  <div className="modal-header">
                    <h3>{editingDocument ? '编辑文档' : '创建新文档'}</h3>
                    <button className="close-btn" onClick={cancelForm}>✖️</button>
                  </div>
                  <form onSubmit={handleSubmit} className="document-form">
                    <div className="form-row">
                      <div className="form-group">
                        <label>文档类型</label>
                        <select
                          name="documentType"
                          value={formData.documentType}
                          onChange={handleInputChange}
                          required
                        >
                          <option value="">选择文档类型</option>
                          <option value="Export License">出口许可证</option>
                          <option value="Import Permit">进口许可证</option>
                          <option value="Certificate of Origin">原产地证书</option>
                          <option value="Customs Declaration">海关申报单</option>
                        </select>
                      </div>
                      <div className="form-group">
                        <label>国家</label>
                        <input
                          type="text"
                          name="country"
                          value={formData.country}
                          onChange={handleInputChange}
                          placeholder="输入国家名称"
                          required
                        />
                      </div>
                    </div>
                    <div className="form-row">
                      <div className="form-group">
                        <label>公司名称</label>
                        <input
                          type="text"
                          name="companyName"
                          value={formData.companyName}
                          onChange={handleInputChange}
                          placeholder="输入公司名称"
                          required
                        />
                      </div>
                      <div className="form-group">
                        <label>价值 (USD)</label>
                        <input
                          type="number"
                          name="value"
                          value={formData.value}
                          onChange={handleInputChange}
                          placeholder="0"
                          min="0"
                          step="0.01"
                          required
                        />
                      </div>
                    </div>
                    <div className="form-row">
                      <div className="form-group">
                        <label>状态</label>
                        <select
                          name="status"
                          value={formData.status}
                          onChange={handleInputChange}
                          required
                        >
                          <option value="Draft">草稿</option>
                          <option value="Pending">待审批</option>
                          <option value="Approved">已批准</option>
                          <option value="Rejected">已拒绝</option>
                        </select>
                      </div>
                      <div className="form-group">
                        <label>风险等级</label>
                        <select
                          name="riskLevel"
                          value={formData.riskLevel}
                          onChange={handleInputChange}
                          required
                        >
                          <option value="Low">低风险</option>
                          <option value="Medium">中风险</option>
                          <option value="High">高风险</option>
                        </select>
                      </div>
                    </div>
                    <div className="form-actions">
                      <button type="button" className="btn btn-secondary" onClick={cancelForm}>
                        取消
                      </button>
                      <button type="submit" className="btn btn-primary">
                        {editingDocument ? '更新文档' : '创建文档'}
                      </button>
                    </div>
                  </form>
                </div>
              </div>
            )}

            <div className="documents-grid">
              {documents.map(doc => (
                <div key={doc.id} className={`document-card status-${doc.status.toLowerCase()}`}>
                  <div className="document-header">
                    <h3>{getStatusIcon(doc.status)} {doc.documentType}</h3>
                    <span 
                      className="risk-badge"
                      style={{ backgroundColor: getRiskColor(doc.riskLevel) }}
                    >
                      {doc.riskLevel} Risk
                    </span>
                  </div>
                  <div className="document-details">
                    <p><strong>ID:</strong> #{doc.id}</p>
                    <p><strong>公司:</strong> {doc.companyName}</p>
                    <p><strong>国家:</strong> {doc.country}</p>
                    <p><strong>价值:</strong> {formatCurrency(doc.value)}</p>
                    <p><strong>状态:</strong> 
                      <span className={`status-badge status-${doc.status.toLowerCase()}`}>
                        {doc.status}
                      </span>
                    </p>
                    <p><strong>创建:</strong> {new Date(doc.createdDate).toLocaleDateString()}</p>
                  </div>
                  <div className="document-actions">
                    <button 
                      className="btn btn-primary"
                      onClick={() => startEdit(doc)}
                    >
                      ✏️ 编辑
                    </button>
                    <button 
                      className="btn btn-danger"
                      onClick={() => deleteDocument(doc.id)}
                    >
                      🗑️ 删除
                    </button>
                  </div>
                </div>
              ))}
            </div>
          </section>
        )}

        {selectedView === 'compliance' && (
          <section className="compliance-section">
            <h2>🛡️ 合规监控中心</h2>
            <div className="compliance-grid">
              <div className="compliance-card">
                <h3>风险等级分布</h3>
                <div className="risk-distribution">
                  <div className="risk-item">
                    <span className="risk-dot high"></span>
                    <span>高风险: {documents.filter(d => d.riskLevel === 'High').length} 个</span>
                  </div>
                  <div className="risk-item">
                    <span className="risk-dot medium"></span>
                    <span>中风险: {documents.filter(d => d.riskLevel === 'Medium').length} 个</span>
                  </div>
                  <div className="risk-item">
                    <span className="risk-dot low"></span>
                    <span>低风险: {documents.filter(d => d.riskLevel === 'Low').length} 个</span>
                  </div>
                </div>
              </div>
              
              <div className="compliance-card">
                <h3>国家合规状态</h3>
                <div className="country-list">
                  {Array.from(new Set(documents.map(d => d.country))).map(country => (
                    <div key={country} className="country-item">
                      <span className="country-flag">🌍</span>
                      <span>{country}</span>
                      <span className="compliance-status">✅ 合规</span>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </section>
        )}
      </main>

      <footer className="app-footer">
        <p>🚀 全栈演示: React + TypeScript ↔ .NET Core Web API</p>
        <p>💻 前端: http://18.183.240.121:3000 | 后端: http://18.183.240.121:5000</p>
        <p>🔧 开发环境: WSL + VSCode + Podman</p>
      </footer>
    </div>
  );
};

export default App;