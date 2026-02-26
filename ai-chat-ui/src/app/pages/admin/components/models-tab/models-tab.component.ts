import { Component, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';

interface AdminModel {
  id: number;
  name: string;
  provider: string;
  modelId: string;
  maxTokens: number;
  status: string;
  availableTo: string;
  icon: string;
}

@Component({
  selector: 'app-models-tab',
  imports: [FormsModule],
  templateUrl: './models-tab.component.html',
  styleUrl: './models-tab.component.scss',
})
export class ModelsTabComponent {
  searchTerm = signal('');

  readonly models = signal<AdminModel[]>([
    { id: 1, name: 'GPT-4o', provider: 'OpenAI', modelId: 'gpt-4o', maxTokens: 128000, status: 'Active', availableTo: 'Everyone', icon: 'bi-stars' },
    { id: 2, name: 'GPT-4o Mini', provider: 'OpenAI', modelId: 'gpt-4o-mini', maxTokens: 128000, status: 'Active', availableTo: 'Everyone', icon: 'bi-star' },
    { id: 3, name: 'Claude 3.5 Sonnet', provider: 'Anthropic', modelId: 'claude-3-5-sonnet-20241022', maxTokens: 200000, status: 'Active', availableTo: 'Everyone', icon: 'bi-chat-square-dots' },
    { id: 4, name: 'Claude 3 Opus', provider: 'Anthropic', modelId: 'claude-3-opus-20240229', maxTokens: 200000, status: 'Active', availableTo: 'Admins Only', icon: 'bi-chat-square-dots' },
    { id: 5, name: 'Gemini 1.5 Pro', provider: 'Google', modelId: 'gemini-1.5-pro', maxTokens: 1048576, status: 'Active', availableTo: 'Users & Admins', icon: 'bi-gem' },
    { id: 6, name: 'Gemini 1.5 Flash', provider: 'Google', modelId: 'gemini-1.5-flash', maxTokens: 1048576, status: 'Active', availableTo: 'Everyone', icon: 'bi-gem' },
    { id: 7, name: 'Llama 3.1 70B', provider: 'Meta', modelId: 'llama-3.1-70b', maxTokens: 131072, status: 'Inactive', availableTo: 'Admins Only', icon: 'bi-robot' },
    { id: 8, name: 'Mistral Large', provider: 'Mistral', modelId: 'mistral-large-latest', maxTokens: 128000, status: 'Active', availableTo: 'Users & Admins', icon: 'bi-wind' },
  ]);

  filteredModels = computed(() => {
    const search = this.searchTerm().toLowerCase();
    if (!search) {
      return this.models();
    }
    return this.models().filter(
      (m) =>
        m.name.toLowerCase().includes(search) ||
        m.provider.toLowerCase().includes(search) ||
        m.modelId.toLowerCase().includes(search),
    );
  });

  onSearchChange(event: Event): void {
    this.searchTerm.set((event.target as HTMLInputElement).value);
  }

  getStatusClass(status: string): string {
    return status === 'Active' ? 'status-active' : 'status-inactive';
  }

  formatTokenCount(count: number): string {
    if (count >= 1000000) {
      return (count / 1000000).toFixed(1) + 'M';
    }
    return (count / 1000).toFixed(0) + 'K';
  }

  trackByModelId(_index: number, model: AdminModel): number {
    return model.id;
  }
}
