import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

export interface BackupLogEntry {
  id: number;
  databaseName: string;
  backupDate: string;
  backupPath: string;
  status: string;
  errorMessage?: string;
  backupSizeBytes?: number;
  duration?: string;
}

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  lastBackup: BackupLogEntry | undefined;

  backups: BackupLogEntry[] = [];

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.loadLastBackup();
    this.loadBackups();
  }

  loadLastBackup() {
    this.http.get<BackupLogEntry>('/api/Backup/latest').subscribe(
      backup => this.lastBackup = backup
    );
  }

  loadBackups() {
    this.http.get<BackupLogEntry[]>('/api/Backup').subscribe(
      // skip first element, as it is the latest backup
      backups => this.backups = backups.slice(1) 
    );
  }

  formatDuration(duration?: string): string {
    if (!duration) {
      return 'N/A';
    }
  
    const timeParts = duration.split(':').map(Number);
    const hours = timeParts[0];
    const minutes = timeParts[1];
    const seconds = timeParts[2];
  
    let result = '';
  
    if (hours > 0) {
      result += `${hours} hour${hours > 1 ? 's' : ''}, `;
    }
    if (minutes > 0) {
      result += `${minutes} minute${minutes > 1 ? 's' : ''}, `;
    }
    result += `${seconds} second${seconds !== 1 ? 's' : ''}`;
  
    return result;
  }
}
