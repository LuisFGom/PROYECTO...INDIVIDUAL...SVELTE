import apiClient from '@/utils/axios'
import { API_ENDPOINTS } from '@/config/api'

export const errorLogService = {
  async getAll() {
    const response = await apiClient.get(API_ENDPOINTS.ERROR_LOGS)
    return response.data
  },
  
  async create(errorData) {
    const response = await apiClient.post(API_ENDPOINTS.ERROR_LOGS, errorData)
    return response.data
  }
}
