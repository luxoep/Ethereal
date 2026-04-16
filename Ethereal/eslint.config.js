import js from '@eslint/js'
import globals from 'globals'
import react from 'eslint-plugin-react' // 新增：React 核心规则插件
import reactHooks from 'eslint-plugin-react-hooks'
import reactRefresh from 'eslint-plugin-react-refresh'

export default [
    // 1. 全局忽略的目录
    {ignores: ['dist', 'node_modules', 'public']},

    // 2. 核心配置
    {
        files: ['**/*.{js,jsx}'],
        languageOptions: {
            ecmaVersion: 2020,
            globals: globals.browser,
            parserOptions: {
                ecmaVersion: 'latest',
                ecmaFeatures: {jsx: true},
                sourceType: 'module',
            },
        },
        // 自动检测 React 版本
        settings: {
            react: {version: 'detect'},
        },
        // 注册所有需要的插件
        plugins: {
            react, // 新增注册
            'react-hooks': reactHooks,
            'react-refresh': reactRefresh,
        },
        rules: {
            // 继承推荐的基准规则
            ...js.configs.recommended.rules,
            ...react.configs.recommended.rules,
            ...react.configs['jsx-runtime'].rules, // 允许在不 import React 的情况下写 JSX
            ...reactHooks.configs.recommended.rules,

            // Vite 默认规则
            'react-refresh/only-export-components': [
                'warn',
                {allowConstantExport: true},
            ],
            
            // 关掉 prop-types 检查
            'react/prop-types': 'off',
            
            // 不会因为没用到的变量而直接白屏报错
            'no-unused-vars': ['warn', {varsIgnorePattern: '^[A-Z_]'}],
        },
    },
]