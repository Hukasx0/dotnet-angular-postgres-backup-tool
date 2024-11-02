import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

/**
 * Interface representing a backup log entry from the backend API
 * Maps directly to the C# BackupLogEntry model
 */
export interface BackupLogEntry {
  id: number;                  // Unique identifier for the backup entry
  databaseName: string;        // Name of the backed up database
  backupDate: string;         // Timestamp of when the backup was created
  backupPath: string;         // File system path where the backup is stored
  status: number;            // Current status of the backup (InProgress - 0/Success - 1/Failed - 2)
  errorMessage?: string;     // Optional error message if backup failed
  backupSizeBytes?: number;  // Optional size of the backup file in bytes
  duration?: string;         // Optional duration of the backup operation
}

/**
 * Root component of the backup tool application
 * Displays the latest backup status and history of previous backups
 */
@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  // Stores the most recent backup entry
  lastBackup: BackupLogEntry | undefined;
  
  // Stores the history of previous backups (excluding the most recent one)
  backups: BackupLogEntry[] = [];

  /**
   * Initializes the component with HttpClient for API communication
   * @param http - Angular's HttpClient for making HTTP requests
   */
  constructor(private http: HttpClient) {}

  /**
   * Lifecycle hook that is called after component initialization
   * Loads both the latest backup and backup history
   */
  ngOnInit() {
    this.loadLastBackup();
    this.loadBackups();
  }

  /**
   * Fetches the most recent backup entry from the API
   * Updates the lastBackup property with the result
   */
  loadLastBackup() {
    this.http.get<BackupLogEntry>('/api/Backup/latest').subscribe(
      backup => this.lastBackup = backup
    );
  }

  /**
   * Fetches all backup entries from the API
   * Stores all entries except the first one (latest) in the backups array
   * First entry is excluded as it's already displayed separately via lastBackup
   */
  loadBackups() {
    this.http.get<BackupLogEntry[]>('/api/Backup').subscribe(
      // skip first element, as it is the latest backup
      backups => this.backups = backups.slice(1)
    );
  }

  /**
   * Formats the backup duration into a human-readable string
   * Converts "HH:MM:SS" format into "X hours, Y minutes, Z seconds"
   * 
   * @param duration - Duration string in "HH:MM:SS" format
   * @returns Formatted string representing the duration in a readable format
   * @example
   * formatDuration("01:30:45") returns "1 hour, 30 minutes, 45 seconds"
   * formatDuration("00:05:30") returns "5 minutes, 30 seconds"
   */
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
