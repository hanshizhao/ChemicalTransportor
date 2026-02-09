-- =====================================================
-- SiemensCommunicator 初始化数据脚本
-- =====================================================
-- 用途：为 Siemens PLC 通讯模块提供示例数据
-- 使用方法：在数据库初始化后执行此脚本
-- =====================================================

-- =====================================================
-- 1. PLC 标签 (PlcTags)
-- =====================================================
INSERT INTO PlcTags (Name, Color) VALUES
('生产数据', '#2196F3'),    -- 蓝色
('设备状态', '#4CAF50'),    -- 绿色
('报警信息', '#F44336'),    -- 红色
('质量数据', '#FF9800'),    -- 橙色
('能耗数据', '#9C27B0'),    -- 紫色
('温度监控', '#00BCD4'),    -- 青色
('压力监控', '#795548'),    -- 棕色
('流量监控', '#607D8B'),    -- 蓝灰色
('液位监控', '#8BC34A'),    -- 浅绿色
('速度监控', '#E91E63');    -- 粉色

-- =====================================================
-- 2. PLC 连接配置 (PlcConnections)
-- =====================================================
INSERT INTO PlcConnections (Name, CpuType, Ip, Rack, Slot, CacheRefreshInterval, IsActive, CreatedAt, UpdatedAt) VALUES
-- S7-1500 PLC (主生产线)
('主生产线 PLC-01', 40, '192.168.1.100', 0, 1, 1000, 1, datetime('now'), datetime('now')),

-- S7-1200 PLC (包装车间)
('包装车间 PLC-02', 30, '192.168.1.101', 0, 0, 1000, 1, datetime('now'), datetime('now')),

-- S7-1500 PLC (原料处理)
('原料处理 PLC-03', 40, '192.168.1.102', 0, 1, 500, 1, datetime('now'), datetime('now')),

-- S7-1200 PLC (质检单元)
('质检单元 PLC-04', 30, '192.168.1.103', 0, 0, 2000, 1, datetime('now'), datetime('now')),

-- S7-1500 PLC (仓储系统)
('仓储系统 PLC-05', 40, '192.168.1.104', 0, 1, 1000, 0, datetime('now'), datetime('now'));

-- =====================================================
-- 3. PLC 分组 (PlcGroups)
-- =====================================================
INSERT INTO PlcGroups (Name, Description, SortOrder) VALUES
('温度监控', '生产过程中各环节温度监控数据点', 1),
('压力监控', '管道和容器压力监控数据点', 2),
('流量监控', '液体和气体流量监控数据点', 3),
('液位监控', '储罐和容器液位监控数据点', 4),
('电机控制', '各类电机运行状态和控制点', 5),
('阀门控制', '各类阀门开关状态和控制点', 6),
('报警信号', '系统报警和故障信号', 7),
('生产计数', '产品计数和生产统计', 8),
('能耗数据', '电力、水、气等能耗数据', 9),
('设备状态', '设备运行状态和健康指标', 10);

-- =====================================================
-- 4. PLC 数据点 (PlcDataPoints)
-- =====================================================

-- 主生产线 PLC-01 数据点
INSERT INTO PlcDataPoints (
    ConnectionId, GroupId, Name, Description, DataType, DbNumber, StartByte, BitOffset,
    VarType, DataCount, EnableCache, Unit, ConversionFormula, MinValue, MaxValue,
    Tags, CreatedAt, UpdatedAt
) VALUES
-- 温度监控组
(1, 1, '反应釜温度', '主反应釜内部温度', 0, 1, 0, 0, 'Real', 1, 1, '°C', NULL, 0, 200,
 '[{"id":1,"name":"生产数据","color":"#2196F3"},{"id":6,"name":"温度监控","color":"#00BCD4"}]', datetime('now'), datetime('now')),

(1, 1, '预热器温度', '原料预热器温度', 0, 1, 4, 0, 'Real', 1, 1, '°C', NULL, 0, 150,
 '[{"id":1,"name":"生产数据","color":"#2196F3"},{"id":6,"name":"温度监控","color":"#00BCD4"}]', datetime('now'), datetime('now')),

(1, 1, '冷却水温度', '冷却系统回水温度', 0, 1, 8, 0, 'Real', 1, 1, '°C', NULL, 0, 80,
 '[{"id":1,"name":"生产数据","color":"#2196F3"},{"id":6,"name":"温度监控","color":"#00BCD4"}]', datetime('now'), datetime('now')),

-- 压力监控组
(1, 2, '反应釜压力', '主反应釜内部压力', 0, 1, 12, 0, 'Real', 1, 1, 'bar', NULL, 0, 10,
 '[{"id":1,"name":"生产数据","color":"#2196F3"},{"id":7,"name":"压力监控","color":"#795548"}]', datetime('now'), datetime('now')),

(1, 2, '管道压力', '主管道压力', 0, 1, 16, 0, 'Real', 1, 1, 'bar', NULL, 0, 16,
 '[{"id":1,"name":"生产数据","color":"#2196F3"},{"id":7,"name":"压力监控","color":"#795548"}]', datetime('now'), datetime('now')),

-- 流量监控组
(1, 3, '进料流量', '原料进料流量', 0, 1, 20, 0, 'Real', 1, 1, 'L/min', NULL, 0, 500,
 '[{"id":1,"name":"生产数据","color":"#2196F3"},{"id":8,"name":"流量监控","color":"#607D8B"}]', datetime('now'), datetime('now')),

(1, 3, '出料流量', '产品出料流量', 0, 1, 24, 0, 'Real', 1, 1, 'L/min', NULL, 0, 500,
 '[{"id":1,"name":"生产数据","color":"#2196F3"},{"id":8,"name":"流量监控","color":"#607D8B"}]', datetime('now'), datetime('now')),

-- 液位监控组
(1, 4, '原料罐液位', 'A原料储罐液位', 0, 1, 28, 0, 'Int', 1, 1, '%', 'x/10', 0, 100,
 '[{"id":1,"name":"生产数据","color":"#2196F3"},{"id":9,"name":"液位监控","color":"#8BC34A"}]', datetime('now'), datetime('now')),

(1, 4, '产品罐液位', '成品储罐液位', 0, 1, 30, 0, 'Int', 1, 1, '%', 'x/10', 0, 100,
 '[{"id":1,"name":"生产数据","color":"#2196F3"},{"id":9,"name":"液位监控","color":"#8BC34A"}]', datetime('now'), datetime('now')),

-- 电机控制组
(1, 5, '主电机运行', '主电机运行状态', 0, 1, 32, 0, 'Bool', 1, 1, NULL, NULL, NULL, NULL,
 '[{"id":2,"name":"设备状态","color":"#4CAF50"}]', datetime('now'), datetime('now')),

(1, 5, '主电机速度', '主电机转速设定', 0, 1, 34, 0, 'Int', 1, 1, 'rpm', 'x*10', 0, 3000,
 '[{"id":1,"name":"生产数据","color":"#2196F3"},{"id":10,"name":"速度监控","color":"#E91E63"}]', datetime('now'), datetime('now')),

(1, 5, '搅拌电机运行', '搅拌电机运行状态', 0, 1, 36, 0, 'Bool', 1, 1, NULL, NULL, NULL, NULL,
 '[{"id":2,"name":"设备状态","color":"#4CAF50"}]', datetime('now'), datetime('now')),

-- 阀门控制组
(1, 6, '进料阀开度', '进料阀门开度百分比', 0, 1, 38, 0, 'Int', 1, 1, '%', NULL, 0, 100,
 '[{"id":1,"name":"生产数据","color":"#2196F3"}]', datetime('now'), datetime('now')),

(1, 6, '出料阀开度', '出料阀门开度百分比', 0, 1, 40, 0, 'Int', 1, 1, '%', NULL, 0, 100,
 '[{"id":1,"name":"生产数据","color":"#2196F3"}]', datetime('now'), datetime('now')),

-- 报警信号组
(1, 7, '高温报警', '温度超高报警信号', 0, 1, 42, 0, 'Bool', 1, 1, NULL, NULL, NULL, NULL,
 '[{"id":3,"name":"报警信息","color":"#F44336"}]', datetime('now'), datetime('now')),

(1, 7, '高压报警', '压力超高报警信号', 0, 1, 42, 1, 'Bool', 1, 1, NULL, NULL, NULL, NULL,
 '[{"id":3,"name":"报警信息","color":"#F44336"}]', datetime('now'), datetime('now')),

(1, 7, '低液位报警', '液位过低报警信号', 0, 1, 42, 2, 'Bool', 1, 1, NULL, NULL, NULL, NULL,
 '[{"id":3,"name":"报警信息","color":"#F44336"}]', datetime('now'), datetime('now')),

-- 生产计数组
(1, 8, '生产计数', '当日产品生产数量', 0, 1, 44, 0, 'DInt', 1, 1, '件', NULL, 0, 100000,
 '[{"id":1,"name":"生产数据","color":"#2196F3"}]', datetime('now'), datetime('now')),

(1, 8, '合格品数', '当日合格品数量', 0, 1, 48, 0, 'DInt', 1, 1, '件', NULL, 0, 100000,
 '[{"id":4,"name":"质量数据","color":"#FF9800"}]', datetime('now'), datetime('now')),

-- 能耗数据组
(1, 9, '主电机电流', '主电机运行电流', 0, 1, 52, 0, 'Real', 1, 1, 'A', NULL, 0, 200,
 '[{"id":5,"name":"能耗数据","color":"#9C27B0"}]', datetime('now'), datetime('now')),

(1, 9, '总功耗', '系统总功耗', 0, 1, 56, 0, 'Real', 1, 1, 'kW', NULL, 0, 500,
 '[{"id":5,"name":"能耗数据","color":"#9C27B0"}]', datetime('now'), datetime('now')),

-- 设备状态组
(1, 10, '设备运行状态', '系统整体运行状态', 0, 1, 60, 0, 'Int', 1, 1, NULL, NULL, 0, 3,
 '[{"id":2,"name":"设备状态","color":"#4CAF50"}]', datetime('now'), datetime('now')),

(1, 10, '设备故障代码', '设备故障代码', 0, 1, 62, 0, 'Int', 1, 1, NULL, NULL, 0, 65535,
 '[{"id":2,"name":"设备状态","color":"#4CAF50"},{"id":3,"name":"报警信息","color":"#F44336"}]', datetime('now'), datetime('now'));

-- 包装车间 PLC-02 数据点
INSERT INTO PlcDataPoints (
    ConnectionId, GroupId, Name, Description, DataType, DbNumber, StartByte, BitOffset,
    VarType, DataCount, EnableCache, Unit, ConversionFormula, MinValue, MaxValue,
    Tags, CreatedAt, UpdatedAt
) VALUES
(2, 5, '包装电机运行', '包装机电机运行状态', 0, 1, 0, 0, 'Bool', 1, 1, NULL, NULL, NULL, NULL,
 '[{"id":2,"name":"设备状态","color":"#4CAF50"}]', datetime('now'), datetime('now')),

(2, 5, '包装电机速度', '包装机运行速度', 0, 1, 2, 0, 'Int', 1, 1, '件/min', 'x*10', 0, 500,
 '[{"id":1,"name":"生产数据","color":"#2196F3"},{"id":10,"name":"速度监控","color":"#E91E63"}]', datetime('now'), datetime('now')),

(2, 8, '包装计数', '当日包装数量', 0, 1, 4, 0, 'DInt', 1, 1, '件', NULL, 0, 50000,
 '[{"id":1,"name":"生产数据","color":"#2196F3"}]', datetime('now'), datetime('now'));

-- 原料处理 PLC-03 数据点
INSERT INTO PlcDataPoints (
    ConnectionId, GroupId, Name, Description, DataType, DbNumber, StartByte, BitOffset,
    VarType, DataCount, EnableCache, Unit, ConversionFormula, MinValue, MaxValue,
    Tags, CreatedAt, UpdatedAt
) VALUES
(3, 1, '原料预热温度', '原料预热罐温度', 0, 1, 0, 0, 'Real', 1, 1, '°C', NULL, 0, 120,
 '[{"id":1,"name":"生产数据","color":"#2196F3"},{"id":6,"name":"温度监控","color":"#00BCD4"}]', datetime('now'), datetime('now')),

(3, 4, '原料A液位', '原料A储罐液位', 0, 1, 4, 0, 'Int', 1, 1, '%', 'x/10', 0, 100,
 '[{"id":1,"name":"生产数据","color":"#2196F3"},{"id":9,"name":"液位监控","color":"#8BC34A"}]', datetime('now'), datetime('now')),

(3, 4, '原料B液位', '原料B储罐液位', 0, 1, 6, 0, 'Int', 1, 1, '%', 'x/10', 0, 100,
 '[{"id":1,"name":"生产数据","color":"#2196F3"},{"id":9,"name":"液位监控","color":"#8BC34A"}]', datetime('now'), datetime('now'));

-- 质检单元 PLC-04 数据点
INSERT INTO PlcDataPoints (
    ConnectionId, GroupId, Name, Description, DataType, DbNumber, StartByte, BitOffset,
    VarType, DataCount, EnableCache, Unit, ConversionFormula, MinValue, MaxValue,
    Tags, CreatedAt, UpdatedAt
) VALUES
(4, 4, '质检产品重量', '产品重量检测结果', 0, 1, 0, 0, 'Real', 1, 1, 'kg', NULL, 0, 50,
 '[{"id":4,"name":"质量数据","color":"#FF9800"}]', datetime('now'), datetime('now')),

(4, 8, '质检合格数', '质检合格产品数', 0, 1, 4, 0, 'DInt', 1, 1, '件', NULL, 0, 50000,
 '[{"id":4,"name":"质量数据","color":"#FF9800"}]', datetime('now'), datetime('now')),

(4, 8, '质检不合格数', '质检不合格产品数', 0, 1, 8, 0, 'DInt', 1, 1, '件', NULL, 0, 5000,
 '[{"id":4,"name":"质量数据","color":"#FF9800"}]', datetime('now'), datetime('now'));

-- 仓储系统 PLC-05 数据点 (未激活连接)
INSERT INTO PlcDataPoints (
    ConnectionId, GroupId, Name, Description, DataType, DbNumber, StartByte, BitOffset,
    VarType, DataCount, EnableCache, Unit, ConversionFormula, MinValue, MaxValue,
    Tags, CreatedAt, UpdatedAt
) VALUES
(5, 8, '入库总数', '当日入库产品总数', 0, 1, 0, 0, 'DInt', 1, 1, '件', NULL, 0, 10000,
 '[{"id":1,"name":"生产数据","color":"#2196F3"}]', datetime('now'), datetime('now')),

(5, 8, '出库总数', '当日出库产品总数', 0, 1, 4, 0, 'DInt', 1, 1, '件', NULL, 0, 10000,
 '[{"id":1,"name":"生产数据","color":"#2196F3"}]', datetime('now'), datetime('now')),

(5, 4, '仓库1号位', '1号仓库储位占用状态', 0, 1, 8, 0, 'Bool', 1, 1, NULL, NULL, NULL, NULL,
 '[{"id":2,"name":"设备状态","color":"#4CAF50"}]', datetime('now'), datetime('now'));

-- =====================================================
-- 数据统计
-- =====================================================
-- 执行完成后，数据库应包含：
-- - 10 个标签 (PlcTags)
-- - 5 个 PLC 连接 (PlcConnections)
-- - 10 个分组 (PlcGroups)
-- - 31 个数据点 (PlcDataPoints)
-- =====================================================
