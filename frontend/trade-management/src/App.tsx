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
            <h2>📄 贸易文档管理</h2>
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
                    <button className="btn btn-primary">查看详情</button>
                    <button className="btn btn-secondary">编辑</button>
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