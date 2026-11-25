import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './styles.css'
import './i18n'
import { Toaster } from 'react-hot-toast'
import App from './App.tsx'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <Toaster position="top-right" />
    <App />
  </StrictMode>,
)
